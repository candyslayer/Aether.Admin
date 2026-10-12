<template>
  <a-config-provider :locale="zhCN" :theme="themeConfig">
    <a-app style="height: 100%">
      <router-view />
    </a-app>
  </a-config-provider>
</template>

<script setup>
import { computed, onMounted } from 'vue'
import { theme } from 'ant-design-vue'
import zhCN from 'ant-design-vue/es/locale/zh_CN'
import dayjs from 'dayjs'
import 'dayjs/locale/zh-cn'
import { useAppStore } from '@/stores/app'

dayjs.locale('zh-cn')

const appStore = useAppStore()

onMounted(() => {
  appStore.initTheme()
})

const themeConfig = computed(() => {
  const isDark = appStore.isDark
  return {
    algorithm: isDark ? theme.darkAlgorithm : theme.defaultAlgorithm,
    token: {
      colorPrimary: appStore.primaryColor || '#1677ff',
      borderRadius: 6,
      colorBgLayout: isDark ? '#000000' : '#f5f7fa',
      colorBgContainer: isDark ? '#141414' : '#ffffff',
      colorBgElevated: isDark ? '#1f1f1f' : '#ffffff'
    },
    components: {
      Layout: {
        headerBg: isDark ? '#141414' : '#ffffff',
        headerColor: isDark ? 'rgba(255, 255, 255, 0.85)' : 'rgba(0, 0, 0, 0.85)',
        siderBg: isDark ? '#001529' : '#ffffff',
        bodyBg: isDark ? '#000000' : '#f5f7fa'
      }
    }
  }
})
</script>

<style>
#app {
  height: 100%;
}
</style>

