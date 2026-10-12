<template>
  <component :is="iconComponent" v-if="iconComponent" />
</template>

<script setup>
import { computed } from 'vue'
import * as Icons from '@ant-design/icons-vue'

const props = defineProps({
  name: {
    type: String,
    default: 'AppstoreOutlined'
  }
})

const iconComponent = computed(() => {
  if (!props.name) return Icons.AppstoreOutlined
  return Icons[props.name] || Icons.AppstoreOutlined
})
</script>

