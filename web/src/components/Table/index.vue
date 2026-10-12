<template>
  <div class="aether-table-wrapper">
    <!-- 顶部操作条 -->
    <div v-if="$slots.toolbar || title" class="table-toolbar">
      <div class="toolbar-title">
        <span v-if="title" class="title-text">{{ title }}</span>
        <slot name="title" />
      </div>
      <div class="toolbar-actions">
        <slot name="toolbar" />
        <a-space class="system-tools">
          <a-tooltip title="刷新数据">
            <a-button type="text" shape="circle" @click="emit('refresh')">
              <template #icon><ReloadOutlined /></template>
            </a-button>
          </a-tooltip>
        </a-space>
      </div>
    </div>

    <!-- 表格主体 -->
    <a-table
      v-bind="$attrs"
      :columns="columns"
      :data-source="dataSource"
      :loading="loading"
      :pagination="pagination"
      :row-key="rowKey"
      :size="size"
    >
      <!-- 透传外部插槽 -->
      <template v-for="(_, slotName) in $slots" #[slotName]="slotProps">
        <slot :name="slotName" v-bind="slotProps" />
      </template>
    </a-table>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { ReloadOutlined } from '@ant-design/icons-vue'

defineProps({
  title: {
    type: String,
    default: ''
  },
  columns: {
    type: Array,
    required: true
  },
  dataSource: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  pagination: {
    type: [Object, Boolean],
    default: () => ({
      showSizeChanger: true,
      showQuickJumper: true,
      pageSizeOptions: ['10', '20', '50', '100'],
      showTotal: total => `共 ${total} 条数据`
    })
  },
  rowKey: {
    type: [String, Function],
    default: 'id'
  },
  size: {
    type: String,
    default: 'middle'
  }
})

const emit = defineEmits(['refresh'])
</script>

<style scoped>
.aether-table-wrapper {
  background: var(--component-bg, #fff);
  border-radius: 8px;
  padding: 16px;
  transition: background 0.25s ease;
}

.table-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.title-text {
  font-size: 16px;
  font-weight: 600;
  color: var(--text-color, #1f2937);
}

.toolbar-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}
</style>

