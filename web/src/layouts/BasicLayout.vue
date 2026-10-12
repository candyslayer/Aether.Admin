<template>
  <a-layout class="basic-layout">
    <!-- 左侧边栏 -->
    <a-layout-sider
      v-model:collapsed="appStore.collapsed"
      :trigger="null"
      collapsible
      :theme="appStore.themeMode === 'dark' ? 'dark' : 'light'"
      :width="240"
      class="layout-sider"
    >
      <!-- Logo 与系统名称 -->
      <div class="logo-box">
        <div class="logo-icon">⚡</div>
        <span v-if="!appStore.collapsed" class="logo-title">Aether Admin</span>
      </div>

      <!-- 动态菜单 -->
      <div class="menu-container">
        <DynamicMenu />
      </div>
    </a-layout-sider>

    <!-- 右侧主体 -->
    <a-layout class="layout-main">
      <!-- 顶部导航栏 -->
      <a-layout-header class="layout-header">
        <HeaderBar />
      </a-layout-header>

      <!-- 标签页导航 -->
      <RouteTabs @refresh="handleRefresh" />

      <!-- 页面视图区域 -->
      <a-layout-content class="layout-content">
        <router-view v-slot="{ Component, route }">
          <transition name="fade-transform" mode="out-in">
            <keep-alive v-if="isRouterAlive">
              <component :is="Component" :key="route.fullPath" />
            </keep-alive>
          </transition>
        </router-view>
      </a-layout-content>
    </a-layout>
  </a-layout>
</template>

<script setup>
import { ref, nextTick } from 'vue'
import { useAppStore } from '@/stores/app'
import DynamicMenu from './components/DynamicMenu.vue'
import HeaderBar from './components/HeaderBar.vue'
import RouteTabs from './components/RouteTabs.vue'

const appStore = useAppStore()

// 局部页面刷新状态
const isRouterAlive = ref(true)

const handleRefresh = () => {
  isRouterAlive.value = false
  nextTick(() => {
    isRouterAlive.value = true
  })
}
</script>

<style scoped>
.basic-layout {
  min-height: 100vh;
}

.layout-sider {
  box-shadow: 2px 0 8px 0 rgba(29, 35, 41, 0.05);
  z-index: 10;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  transition: all 0.25s ease;
}

.logo-box {
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  border-bottom: 1px solid var(--border-color, #f0f0f0);
  padding: 0 16px;
  cursor: pointer;
  user-select: none;
  transition: border-color 0.25s ease;
}

:deep(.ant-layout-sider-dark) .logo-box {
  border-bottom-color: rgba(255, 255, 255, 0.08);
}

.logo-icon {
  font-size: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 8px;
  background: linear-gradient(135deg, #1677ff 0%, #003eb3 100%);
  color: #fff;
}

.logo-title {
  font-size: 18px;
  font-weight: 700;
  letter-spacing: 0.5px;
  background: linear-gradient(90deg, #1677ff, #722ed1);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  white-space: nowrap;
}

.menu-container {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
}

.layout-main {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  background: var(--layout-bg, #f5f7fa);
  transition: background 0.25s ease;
}

.layout-header {
  padding: 0;
  height: 64px;
  line-height: 64px;
  background: var(--component-bg, #ffffff);
  transition: background 0.25s ease;
}

.layout-content {
  flex: 1;
  padding: 20px;
  overflow-y: auto;
}

/* 页面切换动画 */
.fade-transform-leave-active,
.fade-transform-enter-active {
  transition: all 0.25s ease;
}

.fade-transform-enter-from {
  opacity: 0;
  transform: translateY(-8px);
}

.fade-transform-leave-to {
  opacity: 0;
  transform: translateY(8px);
}
</style>

