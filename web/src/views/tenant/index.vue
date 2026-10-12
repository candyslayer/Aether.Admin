<template>
  <div class="tenant-management">
    <!-- 筛选工具卡片 -->
    <a-card :bordered="false" class="search-card">
      <a-form layout="inline" :model="queryParam">
        <a-form-item label="租户名称/编码">
          <a-input
            v-model:value="queryParam.search"
            placeholder="搜索租户"
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

    <!-- 租户列表 -->
    <a-card :bordered="false" style="margin-top: 16px;">
      <div class="table-operations">
        <a-button
          v-permission="'system:tenant:create'"
          type="primary"
          @click="handleOpenModal(null)"
        >
          <template #icon><PlusOutlined /></template>
          新建企业租户
        </a-button>
      </div>

      <a-table
        :columns="columns"
        :data-source="tenantList"
        :loading="loading"
        row-key="id"
        :pagination="false"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'name'">
            <span style="font-weight: 500;">
              <CloudServerOutlined style="color: #1677ff; margin-right: 6px;" />
              {{ record.name }}
            </span>
          </template>

          <template v-else-if="column.key === 'code'">
            <a-tag color="blue">{{ record.code }}</a-tag>
          </template>

          <template v-else-if="column.key === 'status'">
            <a-tag :color="record.status ? 'success' : 'error'">
              {{ record.status ? '正常' : '已冻结' }}
            </a-tag>
          </template>

          <template v-else-if="column.key === 'expireTime'">
            <span>{{ formatDate(record.expireTime, 'YYYY-MM-DD') }}</span>
          </template>

          <template v-else-if="column.key === 'createdAt'">
            <span>{{ formatDate(record.createdAt) }}</span>
          </template>

          <template v-else-if="column.key === 'action'">
            <a-space>
              <a-button
                v-permission="'system:tenant:update'"
                type="link"
                size="small"
                @click="handleOpenModal(record)"
              >
                编辑
              </a-button>
              <a-popconfirm
                title="确定删除此租户？此操作将级联清理该租户下的所有隔离数据！"
                ok-text="确认删除"
                cancel-text="取消"
                ok-type="danger"
                @confirm="handleDelete(record.id)"
              >
                <a-button
                  type="link"
                  size="small"
                  danger
                  :disabled="record.code === 'default'"
                >
                  删除
                </a-button>
              </a-popconfirm>
            </a-space>
          </template>
        </template>
      </a-table>
    </a-card>

    <!-- 新增 / 编辑租户弹窗 -->
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
        <a-form-item label="企业租户名称" name="name">
          <a-input v-model:value="formState.name" placeholder="请输入企业租户全称" />
        </a-form-item>

        <a-form-item label="租户唯一标识编码 (Tenant Code)" name="code">
          <a-input
            v-model:value="formState.code"
            :disabled="!!editingId"
            placeholder="英文字母数字组合，如 tenant_cloud"
          />
        </a-form-item>

        <a-form-item label="租户服务有效期至" name="expireTime">
          <a-date-picker
            v-model:value="formState.expireTime"
            style="width: 100%"
            placeholder="请选择有效期截止日期"
          />
        </a-form-item>

        <a-form-item label="租户状态" name="status">
          <a-switch
            v-model:checked="formState.status"
            checked-children="正常开启"
            un-checked-children="停用冻结"
          />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import dayjs from 'dayjs'
import { PlusOutlined, SearchOutlined, ReloadOutlined, CloudServerOutlined } from '@ant-design/icons-vue'
import { getTenantList, createTenant, updateTenant, deleteTenant } from '@/api/tenant'
import { formatDate } from '@/utils/format'

const loading = ref(false)
const tenantList = ref([])

const queryParam = reactive({
  search: ''
})

const columns = [
  { title: '企业租户名称', dataIndex: 'name', key: 'name', width: 220 },
  { title: '租户编码', dataIndex: 'code', key: 'code', width: 160 },
  { title: '状态', dataIndex: 'status', key: 'status', width: 110, align: 'center' },
  { title: '有效期截止', dataIndex: 'expireTime', key: 'expireTime', width: 150 },
  { title: '入驻时间', dataIndex: 'createdAt', key: 'createdAt', width: 180 },
  { title: '操作', key: 'action', width: 160, align: 'center' }
]

const modalVisible = ref(false)
const modalTitle = ref('新建租户')
const submitLoading = ref(false)
const editingId = ref(null)
const formRef = ref(null)

const formState = reactive({
  name: '',
  code: '',
  expireTime: null,
  status: true
})

const rules = {
  name: [{ required: true, message: '请输入租户名称', trigger: 'blur' }],
  code: [{ required: true, message: '请输入租户编码', trigger: 'blur' }]
}

const fetchData = async () => {
  try {
    loading.value = true
    const res = await getTenantList(queryParam)
    tenantList.value = res || []
  } catch (error) {
    message.error('加载租户列表失败')
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
    modalTitle.value = '编辑租户'
    editingId.value = record.id
    formState.name = record.name
    formState.code = record.code
    formState.expireTime = record.expireTime ? dayjs(record.expireTime) : null
    formState.status = record.status
  } else {
    modalTitle.value = '新建企业租户'
    editingId.value = null
    formState.name = ''
    formState.code = ''
    formState.expireTime = dayjs().add(1, 'year')
    formState.status = true
  }
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()
    submitLoading.value = true

    const payload = {
      name: formState.name,
      code: formState.code,
      expireTime: formState.expireTime ? formState.expireTime.toISOString() : null,
      status: formState.status
    }

    if (editingId.value) {
      await updateTenant(editingId.value, payload)
      message.success('修改租户成功')
    } else {
      await createTenant(payload)
      message.success('创建租户成功')
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
    await deleteTenant(id)
    message.success('删除租户成功')
    await fetchData()
  } catch (error) {
    message.error(error.message || '删除租户失败')
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

