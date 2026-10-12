import { defineStore } from 'pinia'
import { getMenus } from '@/api/auth'
import BasicLayout from '@/layouts/BasicLayout.vue'

// 动态导入所有 views 下的 vue 单文件组件
const viewModules = import.meta.glob('../views/**/*.vue')

/**
 * 根据后端 component 标识解析组件
 */
function resolveComponent(componentPath) {
  if (!componentPath || componentPath === 'BasicLayout' || componentPath === 'Layout') {
    return BasicLayout
  }

  // 规范化路径
  let cleanPath = componentPath.replace(/^\//, '')
  if (!cleanPath.endsWith('.vue')) {
    cleanPath = `${cleanPath}.vue`
  }

  const fullPath = `../views/${cleanPath}`
  if (viewModules[fullPath]) {
    return viewModules[fullPath]
  }

  // 尝试自动补齐 /index.vue
  const indexPath = `../views/${componentPath.replace(/^\//, '')}/index.vue`
  if (viewModules[indexPath]) {
    return viewModules[indexPath]
  }

  console.warn(`[Aether.Router] 未匹配到视图组件: ${componentPath}`)
  return () => import('@/views/error/404.vue')
}

const defaultIcons = {
  '/dashboard': 'DashboardOutlined',
  '/identity': 'UserOutlined',
  '/identity/user': 'TeamOutlined',
  '/organization': 'ApartmentOutlined',
  '/organization/dept': 'ClusterOutlined',
  '/organization/post': 'IdcardOutlined',
  '/authorization': 'SafetyCertificateOutlined',
  '/authorization/role': 'UserSwitchOutlined',
  '/authorization/menu': 'MenuOutlined',
  '/tenant/list': 'CloudServerOutlined',
  '/audit/log': 'HistoryOutlined'
}

function attachIcons(nodes) {
  if (!Array.isArray(nodes)) return
  for (const node of nodes) {
    if (!node.icon && node.path && defaultIcons[node.path]) {
      node.icon = defaultIcons[node.path]
    }
    if (node.children && node.children.length > 0) {
      attachIcons(node.children)
    }
  }
}

/**
 * 将后端返回的菜单树转为 Vue Router 路由表
 */
function transformMenusToRoutes(menuList) {
  const routes = []

  menuList.forEach(menu => {
    // 过滤掉按钮类型(Type === 3)，按钮不需要注册独立路由
    if (menu.type === 3 || menu.status === false) {
      return
    }

    const isDir = menu.type === 1
    const route = {
      path: menu.path || '',
      name: menu.name ? `Route_${menu.id}` : `Route_${Date.now()}`,
      meta: {
        title: menu.name,
        icon: menu.icon || (isDir ? 'FolderOutlined' : 'FileOutlined'),
        permission: menu.permissionCode,
        visible: menu.visible !== false,
        type: menu.type,
        sort: menu.sort || 0
      }
    }

    if (isDir) {
      // 目录节点递归处理子节点
      route.children = menu.children && menu.children.length > 0
        ? transformMenusToRoutes(menu.children)
        : []
    } else {
      // 页面节点绑定真实组件
      route.component = resolveComponent(menu.component)
    }

    routes.push(route)
  })

  return routes
}

export const usePermissionStore = defineStore('permission', {
  state: () => ({
    menus: [],
    dynamicRoutes: [],
    isRoutesLoaded: false
  }),

  actions: {
    /**
     * 生成动态路由
     */
    async generateRoutes() {
      try {
        const rawMenus = await getMenus()
        attachIcons(rawMenus)
        this.menus = rawMenus || []

        // 构建转换为 Vue 路由
        const transformedRoutes = transformMenusToRoutes(this.menus)
        this.dynamicRoutes = transformedRoutes
        this.isRoutesLoaded = true

        return transformedRoutes
      } catch (error) {
        console.error('动态生成路由异常:', error)
        this.menus = []
        this.dynamicRoutes = []
        this.isRoutesLoaded = false
        throw error
      }
    },

    resetRoutes() {
      this.menus = []
      this.dynamicRoutes = []
      this.isRoutesLoaded = false
    }
  }
})

