import request from './request'
import { mockRoles, mockPermissions } from './mockData'

/**
 * 获取角色列表
 */
export async function getRoleList(params) {
  try {
    return await request({
      url: '/api/roles',
      method: 'get',
      params
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      let filtered = [...mockRoles]
      if (params?.search) {
        const keyword = params.search.toLowerCase()
        filtered = filtered.filter(r => r.name.toLowerCase().includes(keyword) || r.code.toLowerCase().includes(keyword))
      }
      return filtered
    }
    throw error
  }
}

/**
 * 新增角色
 */
export async function createRole(data) {
  try {
    return await request({
      url: '/api/roles',
      method: 'post',
      data
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const newRole = {
        ...data,
        id: 'r' + Math.random().toString(36).substring(2, 10),
        type: 2,
        version: 1
      }
      mockRoles.push(newRole)
      return newRole
    }
    throw error
  }
}

/**
 * 编辑角色
 */
export async function updateRole(id, data) {
  try {
    return await request({
      url: `/api/roles/${id}`,
      method: 'put',
      data
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const idx = mockRoles.findIndex(r => r.id === id)
      if (idx !== -1) {
        mockRoles[idx] = { ...mockRoles[idx], ...data }
        return mockRoles[idx]
      }
    }
    throw error
  }
}

/**
 * 删除角色
 */
export async function deleteRole(id) {
  try {
    return await request({
      url: `/api/roles/${id}`,
      method: 'delete'
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const idx = mockRoles.findIndex(r => r.id === id)
      if (idx !== -1) {
        mockRoles.splice(idx, 1)
      }
      return 'OK'
    }
    throw error
  }
}

/**
 * 获取角色分配的权限与数据范围
 */
export async function getRolePermissions(roleId) {
  try {
    return await request({
      url: `/api/roles/${roleId}/permissions`,
      method: 'get'
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      return {
        menuIds: ['m-dash', 'm-identity', 'm-user', 'btn-user-create', 'btn-user-update', 'btn-user-delete', 'm-org', 'm-dept', 'm-post', 'm-auth', 'm-role', 'm-menu-mgr', 'm-audit'],
        scopeType: 1, // All
        departmentIds: []
      }
    }
    throw error
  }
}

/**
 * 保存角色分配的权限
 * @param {string} roleId 
 * @param {object} data { menuIds: [], scopeType: 1-5, departmentIds: [] }
 */
export async function setRolePermissions(roleId, data) {
  try {
    return await request({
      url: `/api/roles/${roleId}/permissions`,
      method: 'put',
      data
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      return 'OK'
    }
    throw error
  }
}

/**
 * 获取系统所有合法权限项编码
 */
export async function getAllPermissionCodes() {
  try {
    return await request({
      url: '/api/permissions',
      method: 'get'
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      return mockPermissions
    }
    throw error
  }
}

