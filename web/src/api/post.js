import request from './request'
import { mockPosts } from './mockData'

/**
 * 获取岗位列表
 */
export async function getPostList(params) {
  try {
    return await request({
      url: '/api/posts',
      method: 'get',
      params
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      let filtered = [...mockPosts]
      if (params?.search) {
        const keyword = params.search.toLowerCase()
        filtered = filtered.filter(p => p.name.toLowerCase().includes(keyword) || p.code.toLowerCase().includes(keyword))
      }
      return filtered
    }
    throw error
  }
}

/**
 * 新增岗位
 */
export async function createPost(data) {
  try {
    return await request({
      url: '/api/posts',
      method: 'post',
      data
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const newPost = {
        ...data,
        id: 'p' + Math.random().toString(36).substring(2, 10),
        version: 1
      }
      mockPosts.push(newPost)
      return newPost
    }
    throw error
  }
}

/**
 * 编辑岗位
 */
export async function updatePost(id, data) {
  try {
    return await request({
      url: `/api/posts/${id}`,
      method: 'put',
      data
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const idx = mockPosts.findIndex(p => p.id === id)
      if (idx !== -1) {
        mockPosts[idx] = { ...mockPosts[idx], ...data }
        return mockPosts[idx]
      }
    }
    throw error
  }
}

/**
 * 删除岗位
 */
export async function deletePost(id) {
  try {
    return await request({
      url: `/api/posts/${id}`,
      method: 'delete'
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const idx = mockPosts.findIndex(p => p.id === id)
      if (idx !== -1) {
        mockPosts.splice(idx, 1)
      }
      return 'OK'
    }
    throw error
  }
}

