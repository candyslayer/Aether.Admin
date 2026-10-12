<template>
  <div class="menu-management">
    <!-- 工具栏 -->
    <a-card :bordered="false" class="toolbar-card">
      <a-row justify="space-between" align="middle">
        <a-col>
          <a-space>
            <a-input-search
              v-model:value="searchKeyword"
              placeholder="搜索菜单名称/路径..."
              style="width: 260px"
              allow-clear
            />
            <a-button @click="expandAll = !expandAll">
              {{ expandAll ? '折叠全部' : '展开全部' }}
            </a-button>
          </a-space>
        </a-col>

        <a-col>
          <a-space>
            <a-button
              v-permission="'system:menu:create'"
              type="primary"
              @click="handleOpenModal(null)"
            >
              <template #icon><PlusOutlined /></template>
              新增根菜单/目录
            </a-button>
            <a-button @click="fetchData">
              <template #icon><ReloadOutlined /></template>
              刷新
            </a-button>
          </a-space>
        </a-col>
      </a-row>
    </a-card>

    <!-- 菜单树形表格 -->
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
              <DynamicIcon :name="record.icon" style="margin-right: 6px; color: #1677ff;" />
              {{ record.name }}
            </span>
          </template>

          <template v-else-if="column.key === 'type'">
            <a-tag v-if="record.type === 1" color="blue">目录</a-tag>
            <a-tag v-else-if="record.type === 2" color="green">菜单</a-tag>
            <a-tag v-else-if="record.type === 3" color="purple">按钮</a-tag>
          </template>

          <template v-else-if="column.key === 'permissionCode'">
            <a-tag v-if="record.permissionCode" color="geekblue">
              {{ record.permissionCode }}
            </a-tag>
            <span v-else style="color: #bfbfbf;">-</span>
          </template>

          <template v-else-if="column.key === 'status'">
            <a-tag :color="record.status ? 'success' : 'error'">
              {{ record.status ? '正常' : '禁用' }}
            </a-tag>
          </template>

          <template v-else-if="column.key === 'visible'">
            <a-tag :color="record.visible ? 'blue' : 'default'">
              {{ record.visible ? '显示' : '隐藏' }}
            </a-tag>
          </template>

          <template v-else-if="column.key === 'action'">
            <a-space>
              <a-button
                v-if="record.type !== 3"
                v-permission="'system:menu:create'"
                type="link"
                size="small"
                @click="handleOpenModal(record, 'add_child')"
              >
                新增子项
              </a-button>
              <a-button
                v-permission="'system:menu:update'"
                type="link"
                size="small"
                @click="handleOpenModal(record, 'edit')"
              >
                编辑
              </a-button>
              <a-popconfirm
                title="确定删除此菜单节点吗？"
                ok-text="确认"
                cancel-text="取消"
                @confirm="handleDelete(record.id)"
              >
                <a-button
                  v-permission="'system:menu:delete'"
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

    <!-- 新增 / 编辑菜单弹窗 -->
    <a-modal
      v-model:open="modalVisible"
      :title="modalTitle"
      :confirm-loading="submitLoading"
      width="640px"
      @ok="handleSubmit"
    >
      <a-form
        ref="formRef"
        :model="formState"
        :rules="rules"
        layout="vertical"
      >
        <a-form-item label="节点类型 (Type)" name="type">
          <a-radio-group v-model:value="formState.type" button-style="solid">
            <a-radio-button :value="1">目录 (Directory)</a-radio-button>
            <a-radio-button :value="2">菜单 (Menu)</a-radio-button>
            <a-radio-button :value="3">按钮 (Button)</a-radio-button>
          </a-radio-group>
        </a-form-item>

        <a-form-item label="上级菜单" name="parentId">
          <a-tree-select
            v-model:value="formState.parentId"
            :tree-data="treeSelectData"
            placeholder="不选则为顶级节点"
            allow-clear
            :field-names="{ label: 'name', value: 'id', children: 'children' }"
            style="width: 100%"
          />
        </a-form-item>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item label="菜单名称" name="name">
              <a-input v-model:value="formState.name" placeholder="请输入节点名称" />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="图标 (Ant Design Icon)" name="icon">
              <a-input v-model:value="formState.icon" placeholder="如: TeamOutlined" />
            </a-form-item>
          </a-col>
        </a-row>

        <a-row v-if="formState.type !== 3" :gutter="16">
          <a-col :span="12">
            <a-form-item label="路由路径 (Path)" name="path">
              <a-input v-model:value="formState.path" placeholder="如: /identity/user" />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="组件标识 (Component)" name="component">
              <a-input v-model:value="formState.component" placeholder="如: identity/user/index" />
            </a-form-item>
          </a-col>
        </a-row>

        <a-form-item label="权限标识 (Permission Code)" name="permissionCode">
          <a-input v-model:value="formState.permissionCode" placeholder="如: system:user:create" />
        </a-form-item>

        <a-row :gutter="16">
          <a-col :span="8">
            <a-form-item label="显示排序" name="sort">
              <a-input-number v-model:value="formState.sort" :min="0" style="width: 100%" />
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item label="是否可见" name="visible">
              <a-switch v-model:checked="formState.visible" checked-children="显示" un-checked-children="隐藏" />
            </a-form-item>
          </a-col>
          <a-col :span="8">
            <a-form-item label="状态" name="status">
              <a-switch v-model:checked="formState.status" checked-children="正常" un-checked-children="禁用" />
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
import { PlusOutlined, ReloadOutlined } from '@ant-design/icons-vue'
import { getMenuList, createMenu, updateMenu, deleteMenu } from '@/api/menu'
import DynamicIcon from '@/components/DynamicIcon.vue'

const loading = ref(false)
const menuTree = ref([])
const searchKeyword = ref('')
const expandAll = ref(true)

const modalVisible = ref(false)
const modalTitle = ref('新增菜单')
const submitLoading = ref(false)
const editingId = ref(null)
const formRef = ref(null)

const formState = reactive({
  name: '',
  type: 2, // 1: Dir, 2: Menu, 3: Button
  parentId: null,
  path: '',
  component: '',
  permissionCode: '',
  icon: '',
  sort: 1,
  visible: true,
  status: true,
  version: 1
})

const rules = {
  name: [{ required: true, message: '请输入名称', trigger: 'blur' }]
}

const columns = [
  { title: '菜单名称', dataIndex: 'name', key: 'name', width: 220 },
  { title: '类型', dataIndex: 'type', key: 'type', width: 90, align: 'center' },
  { title: '路由路径', dataIndex: 'path', key: 'path', width: 180 },
  { title: '组件路径', dataIndex: 'component', key: 'component', width: 180 },
  { title: '权限标识', dataIndex: 'permissionCode', key: 'permissionCode', width: 190 },
  { title: '排序', dataIndex: 'sort', key: 'sort', width: 80, align: 'center' },
  { title: '可见', dataIndex: 'visible', key: 'visible', width: 80, align: 'center' },
  { title: '状态', dataIndex: 'status', key: 'status', width: 80, align: 'center' },
  { title: '操作', key: 'action', width: 200, align: 'center' }
]

const treeSelectData = computed(() => {
  return [
    {
      id: null,
      name: '根级顶级菜单',
      children: menuTree.value
    }
  ]
})

const filterNodes = (nodes, kw) => {
  return nodes
    .filter(n => n.name.includes(kw) || (n.path && n.path.includes(kw)) || (n.children && filterNodes(n.children, kw).length > 0))
    .map(n => ({
      ...n,
      children: n.children ? filterNodes(n.children, kw) : []
    }))
}

const filteredData = computed(() => {
  if (!searchKeyword.value) return menuTree.value
  return filterNodes(menuTree.value, searchKeyword.value)
})

const fetchData = async () => {
  try {
    loading.value = true
    const res = await getMenuList()
    menuTree.value = res || []
  } catch (error) {
    message.error('加载菜单列表失败')
  } finally {
    loading.value = false
  }
}

const handleOpenModal = (record, actionType) => {
  modalVisible.value = true
  if (actionType === 'add_child' && record) {
    modalTitle.value = `在【${record.name}】下新增子节点`
    editingId.value = null
    formState.name = ''
    formState.type = record.type === 1 ? 2 : 3
    formState.parentId = record.id
    formState.path = ''
    formState.component = ''
    formState.permissionCode = ''
    formState.icon = ''
    formState.sort = 1
    formState.visible = true
    formState.status = true
  } else if (actionType === 'edit' && record) {
    modalTitle.value = '编辑菜单节点'
    editingId.value = record.id
    formState.name = record.name
    formState.type = record.type
    formState.parentId = record.parentId
    formState.path = record.path || ''
    formState.component = record.component || ''
    formState.permissionCode = record.permissionCode || ''
    formState.icon = record.icon || ''
    formState.sort = record.sort || 1
    formState.visible = record.visible !== false
    formState.status = record.status !== false
    formState.version = record.version || 1
  } else {
    modalTitle.value = '新增根节点'
    editingId.value = null
    formState.name = ''
    formState.type = 1
    formState.parentId = null
    formState.path = ''
    formState.component = 'BasicLayout'
    formState.permissionCode = ''
    formState.icon = 'FolderOutlined'
    formState.sort = 1
    formState.visible = true
    formState.status = true
  }
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()
    submitLoading.value = true

    const payload = {
      name: formState.name,
      type: formState.type,
      parentId: formState.parentId || null,
      path: formState.path || null,
      component: formState.component || null,
      permissionCode: formState.permissionCode || null,
      sort: formState.sort,
      visible: formState.visible,
      status: formState.status,
      version: formState.version
    }

    if (editingId.value) {
      await updateMenu(editingId.value, payload)
      message.success('更新菜单节点成功')
    } else {
      await createMenu(payload)
      message.success('新增菜单节点成功')
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
    await deleteMenu(id)
    message.success('删除菜单成功')
    await fetchData()
  } catch (error) {
    message.error(error.message || '删除菜单失败')
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

