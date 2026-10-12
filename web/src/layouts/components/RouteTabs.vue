<template>
  <div class="tabs-container">
    <a-tabs
      :active-key="appStore.activeTabKey"
      type="editable-card"
      size="small"
      :hide-add="true"
      @tab-click="handleTabClick"
      @edit="handleTabEdit"
    >
      <a-tab-pane
        v-for="tab in appStore.tabs"
        :key="tab.path"
        :tab="tab.title"
        :closable="tab.closable"
      />
    </a-tabs>

    <div class="tabs-actions">
      <a-dropdown :trigger="['click']">
        <a-button type="text" size="small">
          <template #icon><DownOutlined style="color: var(--text-secondary, #8c8c8c)" /></template>
        </a-button>
        <template #overlay>
          <a-menu @click="handleActionClick">
            <a-menu-item key="refresh">
              <ReloadOutlined /> 刷新当前页
            </a-menu-item>
            <a-menu-item key="closeOther">
              <CloseOutlined /> 关闭其他标签
            </a-menu-item>
            <a-menu-item key="closeAll">
              <StopOutlined /> 关闭所有标签
            </a-menu-item>
          </a-menu>
        </template>
      </a-dropdown>
    </div>
  </div>
</template>

<script setup>
import { useRouter } from 'vue-router'
import { useAppStore } from '@/stores/app'
import { DownOutlined, ReloadOutlined, CloseOutlined, StopOutlined } from '@ant-design/icons-vue'

const router = useRouter()
const appStore = useAppStore()

const emit = defineEmits(['refresh'])

const handleTabClick = (targetKey) => {
  appStore.activeTabKey = targetKey
  router.push(targetKey)
}

const handleTabEdit = (targetKey, action) => {
  if (action === 'remove') {
    const nextPath = appStore.closeTab(targetKey)
    if (nextPath) {
      router.push(nextPath)
    }
  }
}

const handleActionClick = ({ key }) => {
  if (key === 'refresh') {
    emit('refresh')
  } else if (key === 'closeOther') {
    appStore.closeOtherTabs(appStore.activeTabKey)
  } else if (key === 'closeAll') {
    appStore.closeAllTabs()
    router.push('/dashboard')
  }
}
</script>

<style scoped>
.tabs-container {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 6px 16px 0 16px;
  background: var(--component-bg, #ffffff);
  border-bottom: 1px solid var(--border-color, #f0f0f0);
  transition: background 0.25s ease, border-color 0.25s ease;
}

:deep(.ant-tabs) {
  flex: 1;
  overflow: hidden;
}

:deep(.ant-tabs-nav) {
  margin-bottom: 0 !important;
}

:deep(.ant-tabs-nav::before) {
  border-bottom: 1px solid var(--border-color, #f0f0f0) !important;
}

:deep(.ant-tabs-card .ant-tabs-tab) {
  background: var(--tab-bg, transparent);
  border-color: var(--border-color, #f0f0f0);
  border-radius: 4px 4px 0 0;
  transition: all 0.2s;
  color: var(--text-secondary, rgba(0, 0, 0, 0.65));
}

:deep(.ant-tabs-card .ant-tabs-tab:hover) {
  color: var(--primary-color, #1677ff);
  background: var(--hover-bg, rgba(0, 0, 0, 0.04));
}

:deep(.ant-tabs-card .ant-tabs-tab-active) {
  background: var(--tab-active-bg, #ffffff) !important;
  border-color: var(--border-color, #f0f0f0) !important;
  border-bottom-color: var(--tab-border-active, #ffffff) !important;
  font-weight: 500;
}

:deep(.ant-tabs-card .ant-tabs-tab-active .ant-tabs-tab-btn) {
  color: var(--primary-color, #1677ff) !important;
}

:deep(.ant-tabs-card .ant-tabs-tab-remove) {
  color: var(--text-secondary, rgba(0, 0, 0, 0.45));
}

:deep(.ant-tabs-card .ant-tabs-tab-remove:hover) {
  color: var(--text-color, rgba(0, 0, 0, 0.85));
}

.tabs-actions {
  padding-bottom: 6px;
  margin-left: 8px;
}
</style>

