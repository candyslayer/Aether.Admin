<template>
  <div class="dept-management">
    <!-- 检索与工具栏 -->
    <a-card :bordered="false" class="toolbar-card">
      <a-row :gutter="16" justify="space-between" align="middle">
        <a-col :xs="24" :sm="12">
          <a-space>
            <a-input-search
              v-model:value="searchKeyword"
              placeholder="搜索部门名称..."
              style="width: 260px"
              allow-clear
            />
            <a-button @click="expandAll = !expandAll">
              {{ expandAll ? '折叠全部' : '展开全部' }}
            </a-button>
          </a-space>
        </a-col>

        <a-col :xs="24" :sm="12" style="text-align: right">
          <a-space>
            <a-button
              v-permission="'system:dept:create'"
              type="primary"
              @click="handleOpenModal(null)"
            >
              <template #icon><PlusOutlined /></template>
              新增部门
            </a-button>
            <a-button @click="fetchData">
              <template #icon><ReloadOutlined /></template>
              刷新
            </a-button>
          </a-space>
        </a-col>
      </a-row>
    </a-card>

    <!-- 树形数据表格 -->
    <a-card :bordered="false" style="margin-top: 16px;">
      <a-table
        :columns="columns"
        :data-source="filteredData"
        :loading="loading"
        row-key="id"
        :pagination="false"
        :default-expand-all-rows="expandAll"
        :key="expandAll ? 'expanded' : 'collapsed'"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'name'">
            <span style="font-weight: 500;">
              <ApartmentOutlined style="color: #1677ff; margin-right: 6px;" />
              {{ record.name }}
            </span>
          </template>

          <template v-else-if="column.key === 'status'">
            <a-tag :color="record.status ? 'success' : 'error'">
              {{ record.status ? '正常' : '停用' }}
            </a-tag>
          </template>

          <template v-else-if="column.key === 'action'">
            <a-space>
              <a-button
                v-permission="'system:dept:create'"
                type="link"
                size="small"
                @click="handleOpenModal(record, 'add_child')"
              >
                新增子部门
              </a-button>
              <a-button
                v-permission="'system:dept:update'"
                type="link"
                size="small"
                @click="handleOpenModal(record, 'edit')"
              >
                编辑
              </a-button>
              <a-popconfirm
                title="确定删除此部门吗？删除后其子部门也将受到影响。"
                ok-text="确认"
                cancel-text="取消"
                @confirm="handleDelete(record.id)"
              >
                <a-button
                  v-permission="'system:dept:delete'"
                  type="link"
                  size="small"
                  danger
                >
                  删除
                </a-button>
              </a-popconfirm>
            </a-space>
          </template>
        </template>
      </a-table>
    </a-card>

    <!-- 新增 / 编辑部门弹窗 -->
    <a-modal
      v-model:open="modalVisible"
      :title="modalTitle"
      :confirm-loading="submitLoading"
      @ok="handleSubmit"
    >
      <a-form
        ref="formRef"
        :model="formState"
        :rules="rules"
        layout="vertical"
      >
        <a-form-item label="上级部门" name="parentId">
          <a-tree-select
            v-model:value="formState.parentId"
            :tree-data="treeSelectData"
            placeholder="请选择上级部门"
            allow-clear
            show-search
            tree-node-filter-prop="name"
            :tree-default-expand-all="true"
            :field-names="{ label: 'name', value: 'id', children: 'children' }"
            style="width: 100%"
            @change="handleParentChange"
          />
        </a-form-item>

        <a-form-item label="所属组织" name="organizationId">
          <a-select v-model:value="formState.organizationId" placeholder="请选择所属组织单位">
            <a-select-option v-for="org in orgList" :key="org.id" :value="org.id">
              {{ org.name }}
            </a-select-option>
          </a-select>
        </a-form-item>

        <a-form-item label="部门名称" name="name">
          <a-input v-model:value="formState.name" placeholder="请输入部门名称" />
        </a-form-item>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item label="显示排序" name="sort">
              <a-input-number v-model:value="formState.sort" :min="0" style="width: 100%" />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="部门状态" name="status">
              <a-switch
                v-model:checked="formState.status"
                checked-children="正常"
                un-checked-children="停用"
              />
            </a-form-item>
          </a-col>
        </a-row>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import { PlusOutlined, ReloadOutlined, ApartmentOutlined } from '@ant-design/icons-vue'
import { getDeptTree, createDept, updateDept, deleteDept, getOrganizationList } from '@/api/dept'

const loading = ref(false)
const deptTree = ref([])
const orgList = ref([])
const searchKeyword = ref('')
const expandAll = ref(true)

const modalVisible = ref(false)
const modalTitle = ref('新增部门')
const submitLoading = ref(false)
const editingId = ref(null)
const formRef = ref(null)

const formState = reactive({
  name: '',
  organizationId: '',
  parentId: '0',
  sort: 1,
  status: true,
  version: 1
})

const rules = {
  name: [{ required: true, message: '请输入部门名称', trigger: 'blur' }],
  organizationId: [{ required: true, message: '请选择所属组织', trigger: 'change' }]
}

const columns = [
  { title: '部门名称', dataIndex: 'name', key: 'name', width: 280 },
  { title: '层级路径', dataIndex: 'treePath', key: 'treePath', width: 140 },
  { title: '排序', dataIndex: 'sort', key: 'sort', width: 80, align: 'center' },
  { title: '状态', dataIndex: 'status', key: 'status', width: 100, align: 'center' },
  { title: '操作', key: 'action', width: 220, align: 'center' }
]

// 递归查找指定节点
const findNode = (nodes, id) => {
  for (const n of nodes) {
    if (n.id === id) return n
    if (n.children && n.children.length > 0) {
      const found = findNode(n.children, id)
      if (found) return found
    }
  }
  return null
}

// 构造上级部门下拉树数据，支持“顶级部门”选项，并在编辑时禁用自身及下级子节点防止循环依赖
const treeSelectData = computed(() => {
  const disableNodes = (nodes, targetId) => {
    return nodes.map(node => {
      const isTarget = node.id === targetId
      return {
        ...node,
        disabled: isTarget,
        children: node.children && node.children.length > 0
          ? (isTarget ? disableAll(node.children) : disableNodes(node.children, targetId))
          : []
      }
    })
  }

  const disableAll = (nodes) => {
    return nodes.map(node => ({
      ...node,
      disabled: true,
      children: node.children && node.children.length > 0 ? disableAll(node.children) : []
    }))
  }

  const tree = editingId.value
    ? disableNodes(deptTree.value, editingId.value)
    : deptTree.value

  return [
    {
      id: '0',
      name: '顶级部门',
      children: tree
    }
  ]
})

// 选择上级部门时，自动同步对应所属组织
const handleParentChange = (val) => {
  if (val && val !== '0') {
    const parentNode = findNode(deptTree.value, val)
    if (parentNode && parentNode.organizationId) {
      formState.organizationId = parentNode.organizationId
    }
  }
}

// 部门关键字过滤
const filterNodes = (nodes, kw) => {
  return nodes
    .filter(n => n.name.includes(kw) || (n.children && filterNodes(n.children, kw).length > 0))
    .map(n => ({
      ...n,
      children: n.children ? filterNodes(n.children, kw) : []
    }))
}

const filteredData = computed(() => {
  if (!searchKeyword.value) return deptTree.value
  return filterNodes(deptTree.value, searchKeyword.value)
})

const fetchData = async () => {
  try {
    loading.value = true
    const [tree, orgs] = await Promise.all([getDeptTree(), getOrganizationList()])
    deptTree.value = tree || []
    orgList.value = orgs || []
    if (orgList.value.length > 0 && !formState.organizationId) {
      formState.organizationId = orgList.value[0].id
    }
  } catch (error) {
    message.error('加载部门数据失败')
  } finally {
    loading.value = false
  }
}

const handleOpenModal = (record, type) => {
  modalVisible.value = true
  if (type === 'add_child' && record) {
    modalTitle.value = `新增【${record.name}】的子部门`
    editingId.value = null
    formState.name = ''
    formState.parentId = record.id
    formState.organizationId = record.organizationId || (orgList.value[0]?.id)
    formState.sort = 1
    formState.status = true
  } else if (type === 'edit' && record) {
    modalTitle.value = '编辑部门'
    editingId.value = record.id
    formState.name = record.name
    formState.parentId = record.parentId || '0'
    formState.organizationId = record.organizationId || (orgList.value[0]?.id)
    formState.sort = record.sort
    formState.status = record.status
    formState.version = record.version || 1
  } else {
    modalTitle.value = '新增部门'
    editingId.value = null
    formState.name = ''
    formState.parentId = '0'
    formState.organizationId = orgList.value[0]?.id || ''
    formState.sort = 1
    formState.status = true
  }
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()
    submitLoading.value = true

    const payload = {
      ...formState,
      parentId: formState.parentId && formState.parentId !== '0' ? formState.parentId : null
    }

    if (editingId.value) {
      await updateDept(editingId.value, payload)
      message.success('修改部门成功')
    } else {
      await createDept(payload)
      message.success('新增部门成功')
    }

    modalVisible.value = false
    await fetchData()
  } catch (err) {
    if (err.errorFields) return
    message.error(err.message || '操作失败')
  } finally {
    submitLoading.value = false
  }
}

const handleDelete = async (id) => {
  try {
    await deleteDept(id)
    message.success('删除部门成功')
    await fetchData()
  } catch (error) {
    message.error(error.message || '删除失败，可能存在子部门或关联用户')
  }
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.toolbar-card {
  border-radius: 8px;
}
</style>

