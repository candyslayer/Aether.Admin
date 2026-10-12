<template>
  <a-menu
    v-model:selectedKeys="selectedKeys"
    v-model:openKeys="openKeys"
    mode="inline"
    :theme="appStore.themeMode === 'dark' ? 'dark' : 'light'"
    @click="handleMenuClick"
  >
    <template v-for="item in menuTree" :key="item.path || item.id">
      <!-- 带有子菜单的目录 (Type === 1) -->
      <a-sub-menu
        v-if="item.children && item.children.filter(c => c.visible !== false && c.type !== 3).length > 0"
        :key="item.path || item.id"
      >
        <template #icon>
          <DynamicIcon :name="item.icon" />
        </template>
        <template #title>{{ item.name }}</template>

        <template v-for="child in item.children" :key="child.path || child.id">
          <!-- 递归二级或叶子菜单 -->
          <a-menu-item
            v-if="child.visible !== false && child.type !== 3"
            :key="child.path"
          >
            <template #icon>
              <DynamicIcon :name="child.icon" />
            </template>
            {{ child.name }}
          </a-menu-item>
        </template>
      </a-sub-menu>

      <!-- 单级页面菜单 (Type === 2 且无子菜单) -->
      <a-menu-item
        v-else-if="item.visible !== false && item.type !== 3"
        :key="item.path"
      >
        <template #icon>
          <DynamicIcon :name="item.icon" />
        </template>
        {{ item.name }}
      </a-menu-item>
    </template>
  </a-menu>
</template>

<script setup>
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { usePermissionStore } from '@/stores/permission'
import { useAppStore } from '@/stores/app'
import DynamicIcon from '@/components/DynamicIcon.vue'

const route = useRoute()
const router = useRouter()
const permissionStore = usePermissionStore()
const appStore = useAppStore()

const menuTree = computed(() => permissionStore.menus)

const selectedKeys = ref([route.path])
const openKeys = ref([])

/**
 * 递归查找当前路径的所有父级节点 Key
 */
function findParentKeys(nodes, targetPath, parentChain = []) {
  if (!Array.isArray(nodes)) return null
  for (const node of nodes) {
    const nodeKey = node.path || node.id
    if (node.path === targetPath) {
      return parentChain
    }
    if (node.children && node.children.length > 0) {
      const found = findParentKeys(node.children, targetPath, [...parentChain, nodeKey])
      if (found) return found
    }
  }
  return null
}

// 监听路由与菜单数据变化，自动高亮选中项并保持父级目录展开
watch(
  () => [route.path, menuTree.value],
  ([currentPath]) => {
    if (!currentPath) return
    selectedKeys.value = [currentPath]

    // 自动匹配父级目录的展开 key
    const activeParents = findParentKeys(menuTree.value, currentPath)
    if (activeParents && activeParents.length > 0) {
      openKeys.value = Array.from(new Set([...openKeys.value, ...activeParents]))
    } else {
      // 兼容根据一级路径前缀解析
      const parts = currentPath.split('/').filter(Boolean)
      if (parts.length > 1) {
        const parentPath = `/${parts[0]}`
        openKeys.value = Array.from(new Set([...openKeys.value, parentPath]))
      }
    }
  },
  { immediate: true, deep: true }
)

const handleMenuClick = ({ key }) => {
  if (key && key.startsWith('/')) {
    router.push(key)
  }
}
</script>
