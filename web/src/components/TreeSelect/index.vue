<template>
  <a-tree-select
    v-bind="$attrs"
    :value="modelValue"
    :tree-data="treeData"
    :placeholder="placeholder"
    :allow-clear="allowClear"
    :show-search="showSearch"
    :tree-default-expand-all="treeDefaultExpandAll"
    :field-names="fieldNames"
    style="width: 100%"
    @update:value="handleUpdateValue"
  />
</template>

<script setup>
defineProps({
  modelValue: {
    type: [String, Number, Array],
    default: undefined
  },
  treeData: {
    type: Array,
    default: () => []
  },
  placeholder: {
    type: String,
    default: '请选择部门/上级节点'
  },
  allowClear: {
    type: Boolean,
    default: true
  },
  showSearch: {
    type: Boolean,
    default: true
  },
  treeDefaultExpandAll: {
    type: Boolean,
    default: false
  },
  fieldNames: {
    type: Object,
    default: () => ({
      children: 'children',
      label: 'name',
      value: 'id'
    })
  }
})

const emit = defineEmits(['update:modelValue', 'change'])

const handleUpdateValue = (val) => {
  emit('update:modelValue', val)
  emit('change', val)
}
</script>

