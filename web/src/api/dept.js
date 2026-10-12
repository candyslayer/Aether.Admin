import request from './request'
import { mockDepartments, mockOrganizations } from './mockData'

/**
 * 获取部门树
 */
export async function getDeptTree() {
  try {
    return await request({
      url: '/api/depts/tree',
      method: 'get'
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      return mockDepartments
    }
    throw error
  }
}

/**
 * 新增部门
 */
export async function createDept(data) {
  try {
    return await request({
      url: '/api/depts',
      method: 'post',
      data
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      const newDept = {
        ...data,
        id: 'd' + Math.random().toString(36).substring(2, 10),
        treePath: '/1/' + Date.now(),
        version: 1,
        children: []
      }
      if (data.parentId) {
        function add(nodes) {
          for (const node of nodes) {
            if (node.id === data.parentId) {
              node.children = node.children || []
              node.children.push(newDept)
              return true
            }
            if (node.children && add(node.children)) return true
          }
          return false
        }
        add(mockDepartments)
      } else {
        mockDepartments.push(newDept)
      }
      return newDept
    }
    throw error
  }
}

/**
 * 修改部门
 */
export async function updateDept(id, data) {
  try {
    return await request({
      url: `/api/depts/${id}`,
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
      return updateNode(mockDepartments)
    }
    throw error
  }
}

/**
 * 删除部门
 */
export async function deleteDept(id) {
  try {
    return await request({
      url: `/api/depts/${id}`,
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
      removeNode(mockDepartments)
      return 'OK'
    }
    throw error
  }
}

/**
 * 获取组织架构列表 (企业法人/组织单位)
 */
export async function getOrganizationList() {
  try {
    return await request({
      url: '/api/organizations',
      method: 'get'
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      return mockOrganizations
    }
    throw error
  }
}

