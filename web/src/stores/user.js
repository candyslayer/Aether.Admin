import { defineStore } from 'pinia'
import { login as loginApi, getPermissions as getPermissionsApi } from '@/api/auth'
import { 
  getToken, 
  setToken, 
  removeToken, 
  getRefreshToken, 
  setRefreshToken, 
  removeRefreshToken,
  getTenantCode,
  setTenantCode,
  clearAuthStorage 
} from '@/utils/token'

export const useUserStore = defineStore('user', {
  state: () => ({
    token: getToken(),
    refreshToken: getRefreshToken(),
    tenantCode: getTenantCode(),
    userInfo: null,
    roles: [],
    permissions: []
  }),

  getters: {
    isLoggedIn: state => !!state.token,
    username: state => state.userInfo?.username || 'Admin',
    nickname: state => state.userInfo?.nickname || '系统管理员',
    userRoles: state => state.roles,
    permissionList: state => state.permissions
  },

  actions: {
    /**
     * 登录
     */
    async login(loginForm) {
      try {
        const res = await loginApi(loginForm)
        
        // 兼容不同的后端返回字段风格
        const token = res.accessToken || res.token
        const refreshToken = res.refreshToken || ''
        const permissions = res.permissions || []
        const roles = res.roles || []
        
        this.token = token
        this.refreshToken = refreshToken
        this.roles = roles
        this.permissions = permissions
        this.tenantCode = loginForm.tenantCode || 'default'

        setToken(token)
        setRefreshToken(refreshToken)
        setTenantCode(this.tenantCode)

        // 组装用户信息对象
        this.userInfo = res.user || {
          id: res.userId,
          username: loginForm.username,
          nickname: loginForm.username === 'admin' ? '超级管理员' : loginForm.username,
          deptId: res.deptId,
          roles,
          permissions
        }

        return res
      } catch (error) {
        throw error
      }
    },

    /**
     * 获取最新权限列表
     */
    async fetchPermissions() {
      try {
        const perms = await getPermissionsApi()
        this.permissions = Array.isArray(perms) ? perms : []
        return this.permissions
      } catch (error) {
        console.error('获取权限列表失败:', error)
        return []
      }
    },

    /**
     * 校验是否具备指定权限代码
     */
    hasPermission(code) {
      if (!code) return true
      if (this.permissions.includes('*') || this.roles.includes('admin')) {
        return true
      }
      return this.permissions.includes(code)
    },

    /**
     * 登出
     */
    logout() {
      this.token = ''
      this.refreshToken = ''
      this.userInfo = null
      this.roles = []
      this.permissions = []
      clearAuthStorage()
    }
  }
})

