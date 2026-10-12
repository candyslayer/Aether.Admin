import { defineStore } from 'pinia'
import { getTenantList } from '@/api/tenant'
import { getTenantCode, setTenantCode } from '@/utils/token'

export const useTenantStore = defineStore('tenant', {
  state: () => ({
    currentTenantCode: getTenantCode() || 'default',
    tenantList: [
      { code: 'default', name: '默认租户 (Default)', status: true },
      { code: 'tenant_east', name: '华东区域子公司', status: true },
      { code: 'tenant_global', name: '国际业务事业群', status: false }
    ],
    loading: false
  }),

  getters: {
    currentTenant: state => {
      return state.tenantList.find(t => t.code === state.currentTenantCode) || {
        code: state.currentTenantCode,
        name: `租户 (${state.currentTenantCode})`
      }
    }
  },

  actions: {
    async fetchTenantList() {
      try {
        this.loading = true
        const list = await getTenantList()
        if (Array.isArray(list) && list.length > 0) {
          this.tenantList = list
        }
      } catch (error) {
        console.warn('获取租户列表失败，使用预置租户列表:', error)
      } finally {
        this.loading = false
      }
    },

    setTenant(tenantCode) {
      this.currentTenantCode = tenantCode
      setTenantCode(tenantCode)
    }
  }
})

