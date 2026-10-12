<template>
  <slot v-if="hasAccess" />
</template>

<script setup>
import { computed } from 'vue'
import { useUserStore } from '@/stores/user'

const props = defineProps({
  value: {
    type: [String, Array],
    required: true
  }
})

const userStore = useUserStore()

const hasAccess = computed(() => {
  if (!props.value) return true
  const list = Array.isArray(props.value) ? props.value : [props.value]
  return list.some(code => userStore.hasPermission(code))
})
</script>

