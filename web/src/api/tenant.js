import request from './request'
import { mockTenants } from './mockData'

/**
 * 获取租户列表
 */
export async function getTenantList(params) {
  try {
    return await request({
      url: '/api/tenants',
      method: 'get',
      params
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      let filtered = [...mockTenants]
      if (params?.search) {
        const keyword = params.search.toLowerCase()
        filtered = filtered.filter(t => t.name.toLowerCase().includes(keyword) || t.code.toLowerCase().includes(keyword))
      }
      return filtered
    }
    throw error
  }
}

/**
 * 新增租户
 */
export async function createTenant(data) {
  try {
    return await request({
      url: '/api/tenants',
      method: 'post',
      data
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const newTenant = {
        ...data,
        id: 't' + Math.random().toString(36).substring(2, 10),
        permissionVersion: 1,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      }
      mockTenants.push(newTenant)
      return newTenant
    }
    throw error
  }
}

/**
 * 修改租户
 */
export async function updateTenant(id, data) {
  try {
    return await request({
      url: `/api/tenants/${id}`,
      method: 'put',
      data
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const idx = mockTenants.findIndex(t => t.id === id)
      if (idx !== -1) {
        mockTenants[idx] = { ...mockTenants[idx], ...data, updatedAt: new Date().toISOString() }
        return mockTenants[idx]
      }
    }
    throw error
  }
}

/**
 * 删除租户
 */
export async function deleteTenant(id) {
  try {
    return await request({
      url: `/api/tenants/${id}`,
      method: 'delete'
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const idx = mockTenants.findIndex(t => t.id === id)
      if (idx !== -1) {
        mockTenants.splice(idx, 1)
      }
      return 'OK'
    }
    throw error
  }
}

