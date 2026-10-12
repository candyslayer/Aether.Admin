import { defineStore } from 'pinia'

const getInitialTheme = () => {
  const saved = localStorage.getItem('aether_theme_mode') || 'light'
  if (typeof document !== 'undefined') {
    if (saved === 'dark') {
      document.documentElement.classList.add('dark')
    } else {
      document.documentElement.classList.remove('dark')
    }
  }
  return saved
}

export const useAppStore = defineStore('app', {
  state: () => ({
    // 主题模式: 'light' | 'dark'
    themeMode: getInitialTheme(),
    // 主题品牌色
    primaryColor: localStorage.getItem('aether_primary_color') || '#1677ff',
    // 侧边栏是否折叠
    collapsed: false,
    // 侧边栏宽度
    siderWidth: 240,
    // 多标签页 Tabs
    tabs: [
      {
        title: '仪表盘',
        path: '/dashboard',
        name: 'Dashboard',
        closable: false
      }
    ],
    // 当前激活的标签 Key
    activeTabKey: '/dashboard'
  }),

  getters: {
    isDark: state => state.themeMode === 'dark'
  },

  actions: {
    initTheme() {
      if (typeof document !== 'undefined') {
        if (this.themeMode === 'dark') {
          document.documentElement.classList.add('dark')
        } else {
          document.documentElement.classList.remove('dark')
        }
      }
    },

    toggleTheme(mode) {
      this.themeMode = mode || (this.themeMode === 'dark' ? 'light' : 'dark')
      localStorage.setItem('aether_theme_mode', this.themeMode)
      this.initTheme()
    },

    setPrimaryColor(color) {
      this.primaryColor = color
      localStorage.setItem('aether_primary_color', color)
    },

    toggleCollapsed() {
      this.collapsed = !this.collapsed
    },

    setCollapsed(val) {
      this.collapsed = val
    },

    addTab(route) {
      const { path, meta, name } = route
      if (!path || path === '/login' || path === '/403' || path === '/404') return

      this.activeTabKey = path
      const exists = this.tabs.some(t => t.path === path)
      if (!exists) {
        this.tabs.push({
          title: meta?.title || name || '新标签',
          path,
          name,
          closable: path !== '/dashboard'
        })
      }
    },

    closeTab(targetKey) {
      if (targetKey === '/dashboard') return null

      const index = this.tabs.findIndex(t => t.path === targetKey)
      if (index === -1) return null

      this.tabs = this.tabs.filter(t => t.path !== targetKey)
      
      // 如果关闭的是当前激活标签，则激活相邻标签
      if (this.activeTabKey === targetKey) {
        const nextTab = this.tabs[index] || this.tabs[index - 1]
        this.activeTabKey = nextTab ? nextTab.path : '/dashboard'
        return this.activeTabKey
      }
      return null
    },

    closeOtherTabs(currentKey) {
      this.tabs = this.tabs.filter(t => t.path === '/dashboard' || t.path === currentKey)
      this.activeTabKey = currentKey
    },

    closeAllTabs() {
      this.tabs = this.tabs.filter(t => !t.closable)
      this.activeTabKey = '/dashboard'
    }
  }
})

