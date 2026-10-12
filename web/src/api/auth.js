import request from './request'
import { mockMenus, mockPermissions, mockUsers } from './mockData'

/**
 * 登录
 * @param {object} data { username, password, tenantCode }
 */
export async function login(data) {
  try {
    return await request({
      url: '/api/auth/login',
      method: 'post',
      data: {
        tenantCode: data.tenantCode || 'default',
        username: data.username,
        password: data.password
      }
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true' && !error.response) {
      console.warn('[Aether.Admin] 后端连接不可用，启用企业级内置沙盒模式运行')
      const targetUser = mockUsers.find(u => u.username === data.username) || mockUsers[0]
      return {
        accessToken: 'mock_jwt_token_' + Date.now(),
        refreshToken: 'mock_refresh_token_' + Date.now(),
        userId: targetUser.id,
        tenantId: '11111111-1111-1111-1111-111111111111',
        deptId: targetUser.deptId,
        roles: ['admin'],
        permissions: mockPermissions,
        user: targetUser
      }
    }
    throw error
  }
}

/**
 * 获取当前登录用户可访问的动态菜单树
 */
export async function getMenus() {
  try {
    return await request({
      url: '/api/auth/menus',
      method: 'get'
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      return mockMenus
    }
    throw error
  }
}

/**
 * 获取当前用户拥有的权限代码列表
 */
export async function getPermissions() {
  try {
    return await request({
      url: '/api/auth/permissions',
      method: 'get'
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      return mockPermissions
    }
    throw error
  }
}

