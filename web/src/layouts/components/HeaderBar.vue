<template>
  <div class="header-bar">
    <!-- 左侧: 折叠按钮 & 面包屑 -->
    <div class="header-left">
      <span class="trigger-btn" @click="appStore.toggleCollapsed">
        <MenuUnfoldOutlined v-if="appStore.collapsed" />
        <MenuFoldOutlined v-else />
      </span>

      <a-breadcrumb class="breadcrumb-nav">
        <a-breadcrumb-item>首页</a-breadcrumb-item>
        <a-breadcrumb-item v-if="currentTitle && currentTitle !== '仪表盘'">
          {{ currentTitle }}
        </a-breadcrumb-item>
      </a-breadcrumb>
    </div>

    <!-- 右侧操作区 -->
    <div class="header-right">
      <!-- 租户切换 -->
      <div class="tenant-selector">
        <a-select
          :value="tenantStore.currentTenantCode"
          style="min-width: 170px"
          size="middle"
          @change="handleTenantChange"
        >
          <template #suffixIcon><CloudServerOutlined style="color: #1677ff" /></template>
          <a-select-option
            v-for="t in tenantStore.tenantList"
            :key="t.code"
            :value="t.code"
          >
            <a-badge :status="t.status ? 'success' : 'error'" :text="t.name" />
          </a-select-option>
        </a-select>
      </div>

      <!-- 主题明暗模式切换 -->
      <a-tooltip :title="appStore.isDark ? '切换至亮色模式' : '切换至暗黑模式'">
        <a-button type="text" shape="circle" @click="appStore.toggleTheme()">
          <template #icon>
            <BulbOutlined v-if="appStore.isDark" style="color: #faad14" />
            <BulbFilled v-else style="color: #64748b" />
          </template>
        </a-button>
      </a-tooltip>

      <!-- 消息通知 -->
      <a-popover placement="bottomRight" trigger="click">
        <template #content>
          <div style="width: 280px; padding: 4px;">
            <div style="font-weight: 600; margin-bottom: 8px; display: flex; justify-content: space-between;">
              <span>系统通知 (3)</span>
              <a href="javascript:;" style="font-size: 12px;">全部已读</a>
            </div>
            <a-list size="small" item-layout="horizontal">
              <a-list-item>
                <a-list-item-meta
                  title="租户配额提醒"
                  description="华东区域租户存储配额已使用 82%"
                />
              </a-list-item>
              <a-list-item>
                <a-list-item-meta
                  title="安全审计通过"
                  description="本周 RBAC 权限变更日志审计完毕"
                />
              </a-list-item>
            </a-list>
          </div>
        </template>
        <a-badge :count="3" :offset="[-2, 6]">
          <a-button type="text" shape="circle">
            <template #icon><BellOutlined style="font-size: 16px; color: var(--text-color, #595959)" /></template>
          </a-button>
        </a-badge>
      </a-popover>

      <!-- 用户信息与下拉菜单 -->
      <a-dropdown placement="bottomRight">
        <div class="user-profile">
          <a-avatar style="background-color: #1677ff; vertical-align: middle">
            {{ userStore.nickname ? userStore.nickname.charAt(0) : 'A' }}
          </a-avatar>
          <span class="user-name">{{ userStore.nickname }}</span>
          <DownOutlined style="font-size: 10px; color: var(--text-secondary, #8c8c8c)" />
        </div>
        <template #overlay>
          <a-menu @click="handleUserMenuClick">
            <a-menu-item key="info">
              <UserOutlined /> 个人中心 ({{ userStore.username }})
            </a-menu-item>
            <a-menu-item key="tenant">
              <CloudServerOutlined /> 当前租户: {{ tenantStore.currentTenant.name }}
            </a-menu-item>
            <a-menu-divider />
            <a-menu-item key="logout" danger>
              <LogoutOutlined /> 退出登录
            </a-menu-item>
          </a-menu>
        </template>
      </a-dropdown>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Modal, message } from 'ant-design-vue'
import {
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  BellOutlined,
  BulbOutlined,
  BulbFilled,
  UserOutlined,
  CloudServerOutlined,
  LogoutOutlined,
  DownOutlined
} from '@ant-design/icons-vue'
import { useUserStore } from '@/stores/user'
import { useAppStore } from '@/stores/app'
import { useTenantStore } from '@/stores/tenant'

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()
const appStore = useAppStore()
const tenantStore = useTenantStore()

const currentTitle = computed(() => route.meta?.title || '')

const handleTenantChange = (code) => {
  tenantStore.setTenant(code)
  message.success(`已切换至租户: ${tenantStore.currentTenant.name}`)
  // 重新加载页面数据
  setTimeout(() => {
    window.location.reload()
  }, 300)
}

const handleUserMenuClick = ({ key }) => {
  if (key === 'logout') {
    Modal.confirm({
      title: '确认退出系统',
      content: '您确定要退出 Aether.Admin 企业管理平台吗？',
      okText: '确认退出',
      cancelText: '取消',
      onOk: () => {
        userStore.logout()
        message.success('已安全退出登录')
        router.push('/login')
      }
    })
  } else if (key === 'info') {
    router.push('/identity/user')
  }
}
</script>

<style scoped>
.header-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  height: 64px;
  padding: 0 24px;
  background: var(--component-bg, #ffffff);
  border-bottom: 1px solid var(--border-color, #f0f0f0);
  transition: background 0.25s ease, border-color 0.25s ease;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 16px;
}

.trigger-btn {
  font-size: 18px;
  cursor: pointer;
  transition: color 0.3s;
  color: var(--text-color, #595959);
}
.trigger-btn:hover {
  color: #1677ff;
}

.breadcrumb-nav {
  margin-left: 8px;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 18px;
}

.tenant-selector {
  display: flex;
  align-items: center;
}

.user-profile {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 6px;
  transition: background 0.3s;
}

.user-profile:hover {
  background: var(--hover-bg, rgba(0, 0, 0, 0.04));
}

.user-name {
  font-size: 14px;
  font-weight: 500;
  color: var(--text-color, #262626);
}
</style>

