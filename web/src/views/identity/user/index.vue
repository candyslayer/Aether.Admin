<template>
  <div class="user-management">
    <!-- 顶部检索区 -->
    <a-card :bordered="false" class="search-card">
      <a-form layout="inline" :model="queryParam">
        <a-form-item label="关键字查询">
          <a-input
            v-model:value="queryParam.search"
            placeholder="账号/昵称/手机号"
            allow-clear
            style="width: 220px"
            @pressEnter="handleSearch"
          />
        </a-form-item>

        <a-form-item label="归属部门">
          <a-tree-select
            v-model:value="queryParam.deptId"
            :tree-data="deptTree"
            placeholder="全系统部门"
            allow-clear
            :field-names="{ label: 'name', value: 'id', children: 'children' }"
            style="width: 220px"
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

    <!-- 用户数据表格 -->
    <a-card :bordered="false" style="margin-top: 16px;">
      <div class="table-operations">
        <a-button
          v-permission="'system:user:create'"
          type="primary"
          @click="handleOpenModal(null)"
        >
          <template #icon><UserAddOutlined /></template>
          新增系统用户
        </a-button>
      </div>

      <a-table
        :columns="columns"
        :data-source="userList"
        :loading="loading"
        :pagination="pagination"
        row-key="id"
        @change="handleTableChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'username'">
            <a-space>
              <a-avatar size="small" style="background-color: #1677ff">
                {{ record.nickname ? record.nickname.charAt(0) : 'U' }}
              </a-avatar>
              <span style="font-weight: 500;">{{ record.username }}</span>
            </a-space>
          </template>

          <template v-else-if="column.key === 'deptId'">
            <a-tag color="cyan">{{ getDeptName(record.deptId) }}</a-tag>
          </template>

          <template v-else-if="column.key === 'roleIds'">
            <a-space wrap>
              <a-tag
                v-for="rid in record.roleIds"
                :key="rid"
                color="blue"
              >
                {{ getRoleName(rid) }}
              </a-tag>
            </a-space>
          </template>

          <template v-else-if="column.key === 'postIds'">
            <a-space wrap>
              <a-tag
                v-for="pid in record.postIds"
                :key="pid"
                color="purple"
              >
                {{ getPostName(pid) }}
              </a-tag>
            </a-space>
          </template>

          <template v-else-if="column.key === 'status'">
            <a-tag :color="record.status ? 'success' : 'error'">
              {{ record.status ? '正常' : '禁用' }}
            </a-tag>
          </template>

          <template v-else-if="column.key === 'action'">
            <a-space>
              <a-button
                v-permission="'system:user:update'"
                type="link"
                size="small"
                @click="handleOpenModal(record)"
              >
                编辑
              </a-button>
              <a-popconfirm
                title="确定删除此用户？"
                ok-text="确认"
                cancel-text="取消"
                @confirm="handleDelete(record.id)"
              >
                <a-button
                  v-permission="'system:user:delete'"
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

    <!-- 新增 / 编辑用户弹窗 -->
    <a-modal
      v-model:open="modalVisible"
      :title="modalTitle"
      :confirm-loading="submitLoading"
      width="680px"
      @ok="handleSubmit"
    >
      <a-form
        ref="formRef"
        :model="formState"
        :rules="rules"
        layout="vertical"
      >
        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item label="登录账号 (Username)" name="username">
              <a-input
                v-model:value="formState.username"
                :disabled="!!editingId"
                placeholder="4-32位账号"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="用户昵称 (Nickname)" name="nickname">
              <a-input v-model:value="formState.nickname" placeholder="显示名称" />
            </a-form-item>
          </a-col>
        </a-row>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item
              label="登录密码"
              name="password"
              :help="editingId ? '不修改密码请留空' : '至少12位强密码 (符合.NET后端校验)'"
            >
              <a-input-password
                v-model:value="formState.password"
                placeholder="密码长度 12-256 位"
              />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="所属部门 (Dept)" name="deptId">
              <a-tree-select
                v-model:value="formState.deptId"
                :tree-data="deptTree"
                placeholder="请选择所属部门"
                :field-names="{ label: 'name', value: 'id', children: 'children' }"
                style="width: 100%"
              />
            </a-form-item>
          </a-col>
        </a-row>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item label="角色授权 (Roles)" name="roleIds">
              <a-select
                v-model:value="formState.roleIds"
                mode="multiple"
                placeholder="请选择分配的角色"
                style="width: 100%"
              >
                <a-select-option v-for="r in roleList" :key="r.id" :value="r.id">
                  {{ r.name }} ({{ r.code }})
                </a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="岗位分配 (Posts)" name="postIds">
              <a-select
                v-model:value="formState.postIds"
                mode="multiple"
                placeholder="请选择岗位"
                style="width: 100%"
              >
                <a-select-option v-for="p in postList" :key="p.id" :value="p.id">
                  {{ p.name }}
                </a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
        </a-row>

        <a-row :gutter="16">
          <a-col :span="12">
            <a-form-item label="联系手机" name="mobile">
              <a-input v-model:value="formState.mobile" placeholder="11位手机号码" />
            </a-form-item>
          </a-col>
          <a-col :span="12">
            <a-form-item label="电子邮箱" name="email">
              <a-input v-model:value="formState.email" placeholder="example@aether.io" />
            </a-form-item>
          </a-col>
        </a-row>

        <a-form-item label="账号状态" name="status">
          <a-switch
            v-model:checked="formState.status"
            checked-children="正常"
            un-checked-children="禁用"
          />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import { SearchOutlined, ReloadOutlined, UserAddOutlined } from '@ant-design/icons-vue'
import { getUserList, createUser, updateUser, deleteUser } from '@/api/user'
import { getDeptTree } from '@/api/dept'
import { getRoleList } from '@/api/role'
import { getPostList } from '@/api/post'

const loading = ref(false)
const userList = ref([])
const deptTree = ref([])
const roleList = ref([])
const postList = ref([])

const queryParam = reactive({
  search: '',
  deptId: undefined
})

const pagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showTotal: (total) => `共 ${total} 名用户`
})

const columns = [
  { title: '登录账号', dataIndex: 'username', key: 'username', width: 160 },
  { title: '昵称', dataIndex: 'nickname', key: 'nickname', width: 140 },
  { title: '所属部门', dataIndex: 'deptId', key: 'deptId', width: 140 },
  { title: '关联角色', dataIndex: 'roleIds', key: 'roleIds', width: 160 },
  { title: '岗位', dataIndex: 'postIds', key: 'postIds', width: 140 },
  { title: '联系手机', dataIndex: 'mobile', key: 'mobile', width: 130 },
  { title: '状态', dataIndex: 'status', key: 'status', width: 90, align: 'center' },
  { title: '操作', key: 'action', width: 140, align: 'center' }
]

const modalVisible = ref(false)
const modalTitle = ref('新增用户')
const submitLoading = ref(false)
const editingId = ref(null)
const formRef = ref(null)

const formState = reactive({
  username: '',
  nickname: '',
  password: '',
  deptId: undefined,
  roleIds: [],
  postIds: [],
  mobile: '',
  email: '',
  status: true,
  version: 1
})

const rules = {
  username: [{ required: true, message: '请输入登录账号', trigger: 'blur' }],
  nickname: [{ required: true, message: '请输入用户昵称', trigger: 'blur' }],
  deptId: [{ required: true, message: '请选择所属部门', trigger: 'change' }]
}

// 映射名字辅助方法
const deptMap = ref({})
function fillDeptMap(nodes) {
  for (const n of nodes) {
    deptMap.value[n.id] = n.name
    if (n.children) fillDeptMap(n.children)
  }
}

const getDeptName = (id) => deptMap.value[id] || '未分配'
const getRoleName = (id) => roleList.value.find(r => r.id === id)?.name || id
const getPostName = (id) => postList.value.find(p => p.id === id)?.name || id

const fetchData = async () => {
  try {
    loading.value = true
    const res = await getUserList({
      page: pagination.current,
      size: pagination.pageSize,
      search: queryParam.search || undefined
    })

    if (res && res.items) {
      userList.value = res.items
      pagination.total = res.total
    } else if (Array.isArray(res)) {
      userList.value = res
      pagination.total = res.length
    }
  } catch (error) {
    message.error('加载用户列表失败')
  } finally {
    loading.value = false
  }
}

const loadMetadata = async () => {
  try {
    const [depts, roles, posts] = await Promise.all([
      getDeptTree(),
      getRoleList(),
      getPostList()
    ])
    deptTree.value = depts || []
    fillDeptMap(deptTree.value)
    roleList.value = roles || []
    postList.value = posts || []
  } catch (err) {
    console.error('加载组织角色元数据失败', err)
  }
}

const handleSearch = () => {
  pagination.current = 1
  fetchData()
}

const handleReset = () => {
  queryParam.search = ''
  queryParam.deptId = undefined
  pagination.current = 1
  fetchData()
}

const handleTableChange = (pag) => {
  pagination.current = pag.current
  pagination.pageSize = pag.pageSize
  fetchData()
}

const handleOpenModal = (record) => {
  modalVisible.value = true
  if (record) {
    modalTitle.value = '编辑用户'
    editingId.value = record.id
    formState.username = record.username
    formState.nickname = record.nickname
    formState.password = ''
    formState.deptId = record.deptId
    formState.roleIds = record.roleIds || []
    formState.postIds = record.postIds || []
    formState.mobile = record.mobile || ''
    formState.email = record.email || ''
    formState.status = record.status
    formState.version = record.version || 1
  } else {
    modalTitle.value = '新增用户'
    editingId.value = null
    formState.username = ''
    formState.nickname = ''
    formState.password = 'AetherUser2026!'
    formState.deptId = deptTree.value[0]?.id
    formState.roleIds = []
    formState.postIds = []
    formState.mobile = ''
    formState.email = ''
    formState.status = true
  }
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()
    
    // 密码校验: 新增或输入了密码时必须12-256字符
    if (!editingId.value || formState.password) {
      if (formState.password.length < 12) {
        message.warning('密码长度必须在 12 位至 256 位之间')
        return
      }
    }

    submitLoading.value = true

    const payload = {
      username: formState.username,
      nickname: formState.nickname,
      password: formState.password || undefined,
      deptId: formState.deptId,
      roleIds: formState.roleIds,
      postIds: formState.postIds,
      mobile: formState.mobile || null,
      email: formState.email || null,
      status: formState.status,
      version: formState.version
    }

    if (editingId.value) {
      await updateUser(editingId.value, payload)
      message.success('更新用户成功')
    } else {
      await createUser(payload)
      message.success('创建用户成功')
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
    await deleteUser(id)
    message.success('删除用户成功')
    await fetchData()
  } catch (error) {
    message.error(error.message || '删除用户失败')
  }
}

onMounted(async () => {
  await loadMetadata()
  await fetchData()
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

