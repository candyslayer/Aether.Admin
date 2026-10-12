<template>
  <div class="login-wrapper">
    <!-- 顶部右侧主题切换按钮 -->
    <div class="theme-switch-btn">
      <a-tooltip :title="appStore.isDark ? '切换至亮色模式' : '切换至暗黑模式'">
        <a-button type="text" shape="circle" size="large" class="theme-btn" @click="appStore.toggleTheme()">
          <template #icon>
            <BulbOutlined v-if="appStore.isDark" style="font-size: 20px; color: #faad14" />
            <BulbFilled v-else style="font-size: 20px; color: #64748b" />
          </template>
        </a-button>
      </a-tooltip>
    </div>

    <div class="login-container">
      <!-- 头部品牌区 -->
      <div class="brand-header">
        <div class="brand-logo">⚡</div>
        <h1 class="brand-title">Aether Admin</h1>
        <p class="brand-desc">新一代企业级应用基础平台 · .NET 10 & Minimal API 强力驱动</p>
      </div>

      <!-- 登录卡片 -->
      <a-card :bordered="false" class="login-card">
        <div class="card-title">账号密码登录</div>

        <a-form
          :model="formState"
          :rules="rules"
          layout="vertical"
          @finish="handleLogin"
        >
          <!-- 租户选择 -->
          <a-form-item label="企业租户 (Tenant)" name="tenantCode">
            <a-select v-model:value="formState.tenantCode" size="large" placeholder="请选择或输入租户编码">
              <template #suffixIcon><CloudServerOutlined style="color: #1677ff" /></template>
              <a-select-option value="default">默认租户 (default)</a-select-option>
              <a-select-option value="tenant_east">华东区域子公司 (tenant_east)</a-select-option>
              <a-select-option value="tenant_global">国际业务事业群 (tenant_global)</a-select-option>
            </a-select>
          </a-form-item>

          <!-- 用户名 -->
          <a-form-item label="用户名 (Username)" name="username">
            <a-input
              v-model:value="formState.username"
              size="large"
              placeholder="请输入管理员账号 (如: admin)"
            >
              <template #prefix><UserOutlined style="color: var(--text-secondary, #8c8c8c)" /></template>
            </a-input>
          </a-form-item>

          <!-- 密码 -->
          <a-form-item label="密码 (Password)" name="password">
            <a-input-password
              v-model:value="formState.password"
              size="large"
              placeholder="请输入登录密码"
            >
              <template #prefix><LockOutlined style="color: var(--text-secondary, #8c8c8c)" /></template>
            </a-input-password>
          </a-form-item>

          <!-- 快速填充提示 -->
          <div class="quick-fill">
            <span class="fill-label">演示预设:</span>
            <a-tag color="blue" class="clickable-tag" @click="fillAdmin">超级管理员 (admin)</a-tag>
            <a-tag color="purple" class="clickable-tag" @click="fillDev">技术架构主管</a-tag>
          </div>

          <!-- 登录按钮 -->
          <a-form-item style="margin-top: 24px">
            <a-button
              type="primary"
              html-type="submit"
              size="large"
              block
              :loading="loading"
              class="login-btn"
            >
              登 录 系 统
            </a-button>
          </a-form-item>
        </a-form>

        <div class="footer-info">
          <span>安全协议: JWT Bearer & RBAC 2.0</span>
          <span>前端版本: v1.0.0</span>
        </div>
      </a-card>
    </div>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { message } from 'ant-design-vue'
import {
  UserOutlined,
  LockOutlined,
  CloudServerOutlined,
  BulbOutlined,
  BulbFilled
} from '@ant-design/icons-vue'
import { useUserStore } from '@/stores/user'
import { useTenantStore } from '@/stores/tenant'
import { useAppStore } from '@/stores/app'

const router = useRouter()
const route = useRoute()
const userStore = useUserStore()
const tenantStore = useTenantStore()
const appStore = useAppStore()

const loading = ref(false)

const formState = reactive({
  tenantCode: 'default',
  username: 'admin',
  password: 'Password123456!'
})

const rules = {
  tenantCode: [{ required: true, message: '请选择租户代码', trigger: 'change' }],
  username: [{ required: true, message: '请输入登录用户名', trigger: 'blur' }],
  password: [{ required: true, message: '请输入密码', trigger: 'blur' }]
}

const fillAdmin = () => {
  formState.tenantCode = 'default'
  formState.username = 'admin'
  formState.password = 'Password123456!'
}

const fillDev = () => {
  formState.tenantCode = 'default'
  formState.username = 'dev_lead'
  formState.password = 'Password123456!'
}

const handleLogin = async () => {
  try {
    loading.value = true
    await userStore.login({
      tenantCode: formState.tenantCode,
      username: formState.username,
      password: formState.password
    })

    tenantStore.setTenant(formState.tenantCode)
    message.success(`欢迎回来，${userStore.nickname || formState.username}！`)

    const redirect = route.query.redirect || '/dashboard'
    router.push(redirect)
  } catch (error) {
    message.error(error.message || '登录失败，请检查账号密码或租户配置')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-wrapper {
  position: relative;
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--login-bg);
  padding: 24px;
  transition: background 0.3s ease;
}

.theme-switch-btn {
  position: absolute;
  top: 20px;
  right: 24px;
  z-index: 10;
}

.theme-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  background: var(--component-bg, #ffffff);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  border: 1px solid var(--border-color, #f0f0f0);
  transition: all 0.3s ease;
}

.theme-btn:hover {
  transform: scale(1.05);
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
}

.login-container {
  width: 100%;
  max-width: 440px;
}

.brand-header {
  text-align: center;
  margin-bottom: 28px;
}

.brand-logo {
  font-size: 32px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 56px;
  height: 56px;
  background: linear-gradient(135deg, #1677ff 0%, #0958d9 100%);
  border-radius: 14px;
  color: #fff;
  box-shadow: 0 8px 20px rgba(22, 119, 255, 0.25);
  margin-bottom: 12px;
}

.brand-title {
  font-size: 26px;
  font-weight: 700;
  color: var(--text-color, #1e293b);
  margin: 0 0 6px 0;
  letter-spacing: -0.5px;
  transition: color 0.3s ease;
}

.brand-desc {
  font-size: 13px;
  color: var(--text-secondary, #64748b);
  margin: 0;
  transition: color 0.3s ease;
}

.login-card {
  border-radius: 12px;
  box-shadow: var(--login-card-shadow);
  padding: 12px;
  background: var(--component-bg, #ffffff);
  border: 1px solid var(--border-color, transparent);
  transition: background 0.3s ease, border-color 0.3s ease, box-shadow 0.3s ease;
}

.card-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-color, #334155);
  margin-bottom: 20px;
  transition: color 0.3s ease;
}

.quick-fill {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: -6px;
  font-size: 12px;
}

.fill-label {
  color: var(--text-secondary, #94a3b8);
}

.clickable-tag {
  cursor: pointer;
  user-select: none;
  transition: transform 0.15s;
}

.clickable-tag:hover {
  transform: translateY(-1px);
}

.login-btn {
  font-weight: 600;
  letter-spacing: 2px;
  border-radius: 6px;
  height: 44px;
}

.footer-info {
  display: flex;
  justify-content: space-between;
  margin-top: 18px;
  font-size: 12px;
  color: var(--text-secondary, #94a3b8);
  border-top: 1px dashed var(--border-color, #f1f5f9);
  padding-top: 14px;
  transition: border-color 0.3s ease, color 0.3s ease;
}
</style>

