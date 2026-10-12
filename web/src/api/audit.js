import request from './request'
import { mockAuditLogs } from './mockData'

/**
 * 分页获取审计操作日志
 * @param {object} params { page, size, method, endpoint, userId, startTime, endTime }
 */
export async function getAuditLogs(params) {
  try {
    return await request({
      url: '/api/audit-logs',
      method: 'get',
      params
    })
  } catch (error) {
    if (import.meta.env.VITE_USE_MOCK_FALLBACK === 'true') {
      let filtered = [...mockAuditLogs]
      if (params?.method) {
        filtered = filtered.filter(l => l.method === params.method)
      }
      if (params?.endpoint) {
        filtered = filtered.filter(l => l.endpoint.includes(params.endpoint))
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

