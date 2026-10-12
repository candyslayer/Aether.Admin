import request from './request'
import { mockUsers } from './mockData'

/**
 * 获取用户分页列表
 * @param {object} params { page, size, search }
 */
export async function getUserList(params) {
  try {
    return await request({
      url: '/api/users',
      method: 'get',
      params
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      let filtered = [...mockUsers]
      if (params?.search) {
        const keyword = params.search.toLowerCase()
        filtered = filtered.filter(u => 
          u.username.toLowerCase().includes(keyword) || 
          u.nickname.toLowerCase().includes(keyword) ||
          (u.mobile && u.mobile.includes(keyword))
        )
      }
      return {
        items: filtered,
        total: filtered.length,
        pageNumber: params?.page || 1,
        pageSize: params?.size || 20
      }
    }
    throw error
  }
}

/**
 * 新增用户
 */
export async function createUser(data) {
  try {
    return await request({
      url: '/api/users',
      method: 'post',
      data
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const newUser = {
        ...data,
        id: 'u' + Math.random().toString(36).substring(2, 10),
        version: 1,
        createdAt: new Date().toISOString()
      }
      mockUsers.unshift(newUser)
      return newUser
    }
    throw error
  }
}

/**
 * 修改用户
 */
export async function updateUser(id, data) {
  try {
    return await request({
      url: `/api/users/${id}`,
      method: 'put',
      data
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const index = mockUsers.findIndex(u => u.id === id)
      if (index !== -1) {
        mockUsers[index] = { ...mockUsers[index], ...data }
        return mockUsers[index]
      }
    }
    throw error
  }
}

/**
 * 删除用户
 */
export async function deleteUser(id) {
  try {
    return await request({
      url: `/api/users/${id}`,
      method: 'delete'
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const index = mockUsers.findIndex(u => u.id === id)
      if (index !== -1) {
        mockUsers.splice(index, 1)
      }
      return 'OK'
    }
    throw error
  }
}

