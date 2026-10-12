import axios from 'axios'
import { message } from 'ant-design-vue'
import { getToken, clearAuthStorage, getTenantCode } from '@/utils/token'

// 创建 Axios 实例
// 注：各 API 模块中路径已包含 /api 前缀，默认留空以走 Vite 代理或反向代理，避免产生 /api/api 重复路径
const rawBaseURL = import.meta.env.VITE_API_BASE_URL || ''
const service = axios.create({
  baseURL: rawBaseURL === '/api' ? '' : rawBaseURL,
  timeout: 15000,
  headers: {
    'Content-Type': 'application/json;charset=utf-8'
  }
})

// 请求拦截器
service.interceptors.request.use(
  config => {
    const token = getToken()
    if (token) {
      config.headers['Authorization'] = `Bearer ${token}`
    }

    const tenantCode = getTenantCode()
    if (tenantCode) {
      config.headers['X-Tenant-Code'] = tenantCode
    }

    return config
  },
  error => {
    return Promise.reject(error)
  }
)

// 响应拦截器
service.interceptors.response.use(
  response => {
    const res = response.data

    // 如果是二进制流 (下载文件等) 直接返回
    if (response.request.responseType === 'blob' || response.request.responseType === 'arraybuffer') {
      return res
    }

    // 后端规范统一结构: { code: 0, message: "OK", data: ... }
    if (res && typeof res === 'object' && 'code' in res) {
      if (res.code === 0 || res.code === 200) {
        return res.data !== undefined ? res.data : res
      } else {
        const errMsg = res.message || '请求处理失败'
        message.error(errMsg)
        return Promise.reject(new Error(errMsg))
      }
    }

    // 默认直接返回数据体
    return res
  },
  error => {
    const { response, message: errorMsg } = error

    if (response) {
      const status = response.status
      const serverMessage = response.data?.message || response.data?.title || ''

      switch (status) {
        case 401: {
          const isLoginPage = window.location.pathname === '/login'
          const tip = (serverMessage && serverMessage !== 'Request rejected.')
            ? serverMessage
            : (isLoginPage ? '账号密码或租户错误' : '登录凭证已失效，请重新登录')
          message.error(tip)
          clearAuthStorage()
          if (!isLoginPage) {
            window.location.href = `/login?redirect=${encodeURIComponent(window.location.pathname + window.location.search)}`
          }
          break
        }
        case 403:
          message.error(serverMessage || '对不起，您没有权限执行此操作 (403)')
          break
        case 404:
          message.error(serverMessage || '所请求的资源不存在 (404)')
          break
        case 409:
          message.warning(serverMessage || '数据版本冲突或关联资源限制 (409)')
          break
        case 429:
          message.warning('请求过于频繁，已被限流保护，请稍后再试')
          break
        case 500:
          message.error(serverMessage || '服务器内部错误，请联系系统管理员 (500)')
          break
        default:
          message.error(serverMessage || `请求发生异常 (${status})`)
      }
    } else if (errorMsg && errorMsg.includes('timeout')) {
      message.error('网络请求超时，请检查网络连接')
    } else {
      message.error('网络连接异常或服务未启动')
    }

    return Promise.reject(error)
  }
)

export default service

