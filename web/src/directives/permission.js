import { useUserStore } from '@/stores/user'

/**
 * 权限指令 v-permission
 * 用法:
 * <a-button v-permission="'system:user:create'">新增</a-button>
 * <a-button v-permission="['system:user:create', 'system:user:update']">编辑/新增</a-button>
 */
export const permission = {
  mounted(el, binding) {
    const { value } = binding
    const userStore = useUserStore()

    if (value) {
      const requiredPermissions = Array.isArray(value) ? value : [value]
      const hasPermission = requiredPermissions.some(perm => userStore.hasPermission(perm))

      if (!hasPermission) {
        el.parentNode && el.parentNode.removeChild(el)
      }
    } else {
      throw new Error("使用 v-permission 指令必须指定权限标识，如 v-permission=\"'system:user:create'\"")
    }
  },
  updated(el, binding) {
    const { value } = binding
    const userStore = useUserStore()

    if (value) {
      const requiredPermissions = Array.isArray(value) ? value : [value]
      const hasPermission = requiredPermissions.some(perm => userStore.hasPermission(perm))

      if (!hasPermission) {
        el.parentNode && el.parentNode.removeChild(el)
      }
    }
  }
}

export default function setupPermissionDirective(app) {
  app.directive('permission', permission)
}

