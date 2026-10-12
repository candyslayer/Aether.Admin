import router from './index'
import { useUserStore } from '@/stores/user'
import { usePermissionStore } from '@/stores/permission'
import { useAppStore } from '@/stores/app'
import { getToken } from '@/utils/token'
import NProgress from 'nprogress'
import 'nprogress/nprogress.css'

NProgress.configure({ showSpinner: false })

const whiteList = ['/login', '/403', '/404']

/**
 * 展平嵌套动态路由并注入到路由根布局或顶层
 */
function registerDynamicRoutes(routes) {
  routes.forEach(route => {
    if (route.children && route.children.length > 0) {
      // 目录或带子路由节点，直接展开注册叶子页面
      registerDynamicRoutes(route.children)
    }
    if (route.component) {
      // 检查是否已注册相同名称或路径，避免覆盖已存在的静态路由
      if (route.path !== '/dashboard' && !router.hasRoute(route.name)) {
        // 关键：注册在 RootLayout (BasicLayout) 的子路由下，确保页面渲染在主布局内容区
        router.addRoute('RootLayout', {
          path: route.path,
          name: route.name,
          component: route.component,
          meta: route.meta
        })
      }
    }
  })
}

router.beforeEach(async (to, from, next) => {
  NProgress.start()

  const userStore = useUserStore()
  const permissionStore = usePermissionStore()
  const hasToken = getToken()

  if (hasToken) {
    if (to.path === '/login') {
      next({ path: '/dashboard' })
      NProgress.done()
    } else {
      if (permissionStore.isRoutesLoaded) {
        // 校验页面按钮/模块级权限
        if (to.meta && to.meta.permission) {
          if (userStore.hasPermission(to.meta.permission)) {
            next()
          } else {
            next({ path: '/403', replace: true })
          }
        } else {
          next()
        }
      } else {
        try {
          // 首次加载或刷新，动态生成路由
          await userStore.fetchPermissions()
          const accessRoutes = await permissionStore.generateRoutes()

          // 递归注册动态路由
          registerDynamicRoutes(accessRoutes)

          // 注册 404 通配路由兜底
          if (!router.hasRoute('CatchAll')) {
            router.addRoute({
              path: '/:pathMatch(.*)*',
              name: 'CatchAll',
              redirect: '/404'
            })
          }

          next({ ...to, replace: true })
        } catch (error) {
          console.error('动态加载路由失败:', error)
          userStore.logout()
          next(`/login?redirect=${encodeURIComponent(to.fullPath)}`)
          NProgress.done()
        }
      }
    }
  } else {
    if (whiteList.includes(to.path)) {
      next()
    } else {
      next(`/login?redirect=${encodeURIComponent(to.fullPath)}`)
      NProgress.done()
    }
  }
})

router.afterEach((to) => {
  NProgress.done()

  const appStore = useAppStore()
  appStore.addTab(to)

  const title = to.meta?.title
  const appTitle = import.meta.env.VITE_APP_TITLE || 'Aether Admin'
  document.title = title ? `${title} - ${appTitle}` : appTitle
})

