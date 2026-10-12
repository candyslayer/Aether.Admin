<template>
  <div class="role-management">
    <!-- 筛选卡片 -->
    <a-card :bordered="false" class="search-card">
      <a-form layout="inline" :model="queryParam">
        <a-form-item label="角色名称/编码">
          <a-input
            v-model:value="queryParam.search"
            placeholder="搜索角色"
            allow-clear
            style="width: 220px"
            @pressEnter="handleSearch"
          />
        </a-form-item>
        <a-form-item>
          <a-space>
            <a-button type="primary" @click="handleSearch">
              <template #icon><SearchOutlined /></template>
              查询
            </a-button>
            <a-button @click="handleReset">
              <template #icon><ReloadOutlined /></template>
              重置
            </a-button>
          </a-space>
        </a-form-item>
      </a-form>
    </a-card>

    <!-- 角色列表 -->
    <a-card :bordered="false" style="margin-top: 16px;">
      <div class="table-operations">
        <a-button
          v-permission="'system:role:create'"
          type="primary"
          @click="handleOpenModal(null)"
        >
          <template #icon><PlusOutlined /></template>
          新增角色
        </a-button>
      </div>

      <a-table
        :columns="columns"
        :data-source="roleList"
        :loading="loading"
        row-key="id"
        :pagination="false"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'code'">
            <a-tag color="blue">{{ record.code }}</a-tag>
          </template>

          <template v-else-if="column.key === 'type'">
            <a-tag :color="record.type === 1 ? 'purple' : 'orange'">
              {{ record.type === 1 ? '系统内置' : '自定义角色' }}
            </a-tag>
          </template>

          <template v-else-if="column.key === 'status'">
            <a-tag :color="record.status ? 'success' : 'error'">
              {{ record.status ? '正常' : '禁用' }}
            </a-tag>
          </template>

          <template v-else-if="column.key === 'action'">
            <a-space>
              <a-button
                v-permission="'system:permission:assign'"
                type="link"
                size="small"
                @click="handleOpenPermissionModal(record)"
              >
                分配权限
              </a-button>
              <a-button
                v-permission="'system:role:update'"
                type="link"
                size="small"
                @click="handleOpenModal(record)"
              >
                编辑
              </a-button>
              <a-popconfirm
                title="确定删除此角色？"
                ok-text="确认"
                cancel-text="取消"
                @confirm="handleDelete(record.id)"
              >
                <a-button
                  v-permission="'system:role:delete'"
                  type="link"
                  size="small"
                  danger
                  :disabled="record.code === 'admin'"
                >
                  删除
                </a-button>
              </a-popconfirm>
            </a-space>
          </template>
        </template>
      </a-table>
    </a-card>

    <!-- 新增 / 编辑角色弹窗 -->
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
        <a-form-item label="角色名称" name="name">
          <a-input v-model:value="formState.name" placeholder="请输入角色名称 (如: 业务合规专员)" />
        </a-form-item>

        <a-form-item label="角色编码" name="code">
          <a-input
            v-model:value="formState.code"
            :disabled="!!editingId"
            placeholder="请输入角色唯一编码 (如: COMPLIANCE_OFFICER)"
          />
        </a-form-item>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item label="排序" name="sort">
              <a-input-number v-model:value="formState.sort" :min="0" style="width: 100%" />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="状态" name="status">
              <a-switch
                v-model:checked="formState.status"
                checked-children="正常"
                un-checked-children="禁用"
              />
            </a-form-item>
          </a-col>
        </a-row>
      </a-form>
    </a-modal>

    <!-- 角色权限分配 Drawer 抽屉 -->
    <a-drawer
      v-model:open="permDrawerVisible"
      :title="`为【${currentRole?.name}】配置功能与数据权限`"
      width="500px"
      :footer-style="{ textAlign: 'right' }"
    >
      <a-spin :spinning="drawerLoading">
        <a-tabs default-active-key="menu">
          <!-- 功能与菜单权限树 -->
          <a-tab-pane key="menu" tab="菜单与按钮权限">
            <div style="margin-bottom: 12px;">
              <a-space>
                <a-checkbox v-model:checked="checkStrictly">父子联动关联</a-checkbox>
                <a-button size="small" @click="expandAllTree = !expandAllTree">
                  {{ expandAllTree ? '折叠全部' : '展开全部' }}
                </a-button>
              </a-space>
            </div>
            
            <a-tree
              v-model:checkedKeys="checkedMenuKeys"
              :tree-data="menuTreeData"
              checkable
              :default-expand-all="expandAllTree"
              :check-strictly="!checkStrictly"
              :field-names="{ title: 'name', key: 'id', children: 'children' }"
            />
          </a-tab-pane>

          <!-- 数据权限范围配置 -->
          <a-tab-pane key="data" tab="数据范围授权">
            <a-form layout="vertical">
              <a-form-item label="数据权限范围 (Data Scope)">
                <a-select v-model:value="dataScopeType" style="width: 100%">
                  <a-select-option :value="1">全部数据权限 (All)</a-select-option>
                  <a-select-option :value="2">自定义部门权限 (Custom)</a-select-option>
                  <a-select-option :value="3">本部门数据权限 (Dept)</a-select-option>
                  <a-select-option :value="4">本部门及以下数据权限 (Dept + Child)</a-select-option>
                  <a-select-option :value="5">仅本人数据权限 (Self)</a-select-option>
                </a-select>
              </a-form-item>

              <a-form-item
                v-if="dataScopeType === 2"
                label="选择受权部门列表"
              >
                <a-tree-select
                  v-model:value="customDeptIds"
                  :tree-data="deptTreeData"
                  multiple
                  tree-checkable
                  placeholder="请勾选授权部门"
                  :field-names="{ label: 'name', value: 'id', children: 'children' }"
                  style="width: 100%"
                />
              </a-form-item>
            </a-form>
          </a-tab-pane>
        </a-tabs>
      </a-spin>

      <template #footer>
        <a-space>
          <a-button @click="permDrawerVisible = false">取消</a-button>
          <a-button type="primary" :loading="savePermLoading" @click="handleSavePermissions">
            保存权限配置
          </a-button>
        </a-space>
      </template>
    </a-drawer>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import { SearchOutlined, ReloadOutlined, PlusOutlined } from '@ant-design/icons-vue'
import { getRoleList, createRole, updateRole, deleteRole, getRolePermissions, setRolePermissions } from '@/api/role'
import { getMenuList } from '@/api/menu'
import { getDeptTree } from '@/api/dept'

const loading = ref(false)
const roleList = ref([])

const queryParam = reactive({
  search: ''
})

const columns = [
  { title: '角色名称', dataIndex: 'name', key: 'name', width: 200 },
  { title: '角色标识编码', dataIndex: 'code', key: 'code', width: 220 },
  { title: '角色类型', dataIndex: 'type', key: 'type', width: 140, align: 'center' },
  { title: '状态', dataIndex: 'status', key: 'status', width: 100, align: 'center' },
  { title: '操作', key: 'action', width: 220, align: 'center' }
]

const modalVisible = ref(false)
const modalTitle = ref('新增角色')
const submitLoading = ref(false)
const editingId = ref(null)
const formRef = ref(null)

const formState = reactive({
  name: '',
  code: '',
  sort: 1,
  status: true,
  version: 1
})

const rules = {
  name: [{ required: true, message: '请输入角色名称', trigger: 'blur' }],
  code: [{ required: true, message: '请输入角色编码', trigger: 'blur' }]
}

// 权限配置抽屉相关状态
const permDrawerVisible = ref(false)
const currentRole = ref(null)
const drawerLoading = ref(false)
const savePermLoading = ref(false)
const checkStrictly = ref(true)
const expandAllTree = ref(true)

const menuTreeData = ref([])
const deptTreeData = ref([])
const checkedMenuKeys = ref([])
const dataScopeType = ref(1)
const customDeptIds = ref([])

const fetchData = async () => {
  try {
    loading.value = true
    const res = await getRoleList(queryParam)
    roleList.value = res || []
  } catch (error) {
    message.error('加载角色列表失败')
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  fetchData()
}

const handleReset = () => {
  queryParam.search = ''
  fetchData()
}

const handleOpenModal = (record) => {
  modalVisible.value = true
  if (record) {
    modalTitle.value = '编辑角色'
    editingId.value = record.id
    formState.name = record.name
    formState.code = record.code
    formState.sort = record.sort || 1
    formState.status = record.status
    formState.version = record.version || 1
  } else {
    modalTitle.value = '新增角色'
    editingId.value = null
    formState.name = ''
    formState.code = ''
    formState.sort = 1
    formState.status = true
  }
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()
    submitLoading.value = true

    if (editingId.value) {
      await updateRole(editingId.value, { ...formState })
      message.success('更新角色成功')
    } else {
      await createRole({ ...formState })
      message.success('新增角色成功')
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
    await deleteRole(id)
    message.success('删除角色成功')
    await fetchData()
  } catch (error) {
    message.error(error.message || '删除角色失败')
  }
}

// 打开权限配置抽屉
const handleOpenPermissionModal = async (role) => {
  currentRole.value = role
  permDrawerVisible.value = true
  drawerLoading.value = true

  try {
    const [menus, depts, grants] = await Promise.all([
      getMenuList(),
      getDeptTree(),
      getRolePermissions(role.id)
    ])

    menuTreeData.value = menus || []
    deptTreeData.value = depts || []

    if (grants) {
      checkedMenuKeys.value = grants.menuIds || []
      dataScopeType.value = grants.scopeType || 1
      customDeptIds.value = grants.departmentIds || []
    }
  } catch (error) {
    message.error('加载权限数据失败')
  } finally {
    drawerLoading.value = false
  }
}

// 提交权限配置
const handleSavePermissions = async () => {
  try {
    savePermLoading.value = true
    const menuIds = Array.isArray(checkedMenuKeys.value) 
      ? checkedMenuKeys.value 
      : (checkedMenuKeys.value?.checked || [])

    const payload = {
      menuIds,
      scopeType: dataScopeType.value,
      departmentIds: dataScopeType.value === 2 ? customDeptIds.value : []
    }

    await setRolePermissions(currentRole.value.id, payload)
    message.success(`成功更新角色【${currentRole.value.name}】的权限体系`)
    permDrawerVisible.value = false
  } catch (error) {
    message.error(error.message || '保存权限配置失败')
  } finally {
    savePermLoading.value = false
  }
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.search-card {
  border-radius: 8px;
}

.table-operations {
  margin-bottom: 16px;
}
</style>

