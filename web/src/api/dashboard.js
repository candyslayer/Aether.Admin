import request from './request'
import { mockUsers, mockDepartments, mockRoles, mockTenants } from './mockData'

/**
 * 获取首页概览统计指标
 */
export async function getDashboardStats() {
  try {
    return await request({
      url: '/api/dashboard/stats',
      method: 'get'
    })
  } catch (error) {
    // 聚合或回退统计数据
    return {
      userCount: mockUsers.length + 128,
      deptCount: 16,
      roleCount: mockRoles.length + 5,
      tenantCount: mockTenants.length,
      todayRequests: 48920,
      systemUptime: '99.99%',
      avgResponseMs: 38
    }
  }
}

/**
 * 获取访问和吞吐量趋势
 */
export async function getVisitTrend() {
  return {
    dates: ['周一', '周二', '周三', '周四', '周五', '周六', '周日'],
    pv: [3200, 4800, 4200, 6800, 7900, 5100, 6200],
    apiCalls: [18000, 24000, 29000, 36000, 48000, 22000, 31000]
  }
}

/**
 * 获取操作日志与模块分布
 */
export async function getAuditDistribution() {
  return [
    { value: 45, name: '用户管理 / Users' },
    { value: 25, name: '组织权限 / Perms' },
    { value: 15, name: '认证授权 / Auth' },
    { value: 10, name: '租户调度 / Tenant' },
    { value: 5, name: '基础设施 / Infra' }
  ]
}

