const TOKEN_KEY = 'aether_access_token'
const REFRESH_TOKEN_KEY = 'aether_refresh_token'
const TENANT_KEY = 'aether_tenant_code'

export function getToken() {
  return localStorage.getItem(TOKEN_KEY) || ''
}

export function setToken(token) {
  if (token) {
    localStorage.setItem(TOKEN_KEY, token)
  } else {
    localStorage.removeItem(TOKEN_KEY)
  }
}

export function removeToken() {
  localStorage.removeItem(TOKEN_KEY)
}

export function getRefreshToken() {
  return localStorage.getItem(REFRESH_TOKEN_KEY) || ''
}

export function setRefreshToken(token) {
  if (token) {
    localStorage.setItem(REFRESH_TOKEN_KEY, token)
  } else {
    localStorage.removeItem(REFRESH_TOKEN_KEY)
  }
}

export function removeRefreshToken() {
  localStorage.removeItem(REFRESH_TOKEN_KEY)
}

export function getTenantCode() {
  return localStorage.getItem(TENANT_KEY) || 'default'
}

export function setTenantCode(code) {
  if (code) {
    localStorage.setItem(TENANT_KEY, code)
  } else {
    localStorage.removeItem(TENANT_KEY)
  }
}

export function clearAuthStorage() {
  removeToken()
  removeRefreshToken()
}

