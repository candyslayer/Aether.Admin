import request from './request'
import { mockMenus } from './mockData'

/**
 * 获取菜单列表树
 */
export async function getMenuList(params) {
  try {
    return await request({
      url: '/api/menus',
      method: 'get',
      params
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      return mockMenus
    }
    throw error
  }
}

/**
 * 新增菜单
 */
export async function createMenu(data) {
  try {
    return await request({
      url: '/api/menus',
      method: 'post',
      data
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const newMenu = {
        ...data,
        id: 'm-' + Math.random().toString(36).substring(2, 8),
        version: 1,
        children: []
      }
      if (data.parentId) {
        function add(nodes) {
          for (const node of nodes) {
            if (node.id === data.parentId) {
              node.children = node.children || []
              node.children.push(newMenu)
              return true
            }
            if (node.children && add(node.children)) return true
          }
          return false
        }
        add(mockMenus)
      } else {
        mockMenus.push(newMenu)
      }
      return newMenu
    }
    throw error
  }
}

/**
 * 编辑菜单
 */
export async function updateMenu(id, data) {
  try {
    return await request({
      url: `/api/menus/${id}`,
      method: 'put',
      data
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      function updateNode(nodes) {
        for (const node of nodes) {
          if (node.id === id) {
            Object.assign(node, data)
            return node
          }
          if (node.children) {
            const res = updateNode(node.children)
            if (res) return res
          }
        }
        return null
      }
      return updateNode(mockMenus)
    }
    throw error
  }
}

/**
 * 删除菜单
 */
export async function deleteMenu(id) {
  try {
    return await request({
      url: `/api/menus/${id}`,
      method: 'delete'
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      function removeNode(nodes) {
        const idx = nodes.findIndex(n => n.id === id)
        if (idx !== -1) {
          nodes.splice(idx, 1)
          return true
        }
        for (const n of nodes) {
          if (n.children && removeNode(n.children)) return true
        }
        return false
      }
      removeNode(mockMenus)
      return 'OK'
    }
    throw error
  }
}

