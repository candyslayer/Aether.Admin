<template>
  <div class="post-management">
    <!-- 筛选卡片 -->
    <a-card :bordered="false" class="search-card">
      <a-form layout="inline" :model="queryParam">
        <a-form-item label="岗位名称/编码">
          <a-input
            v-model:value="queryParam.search"
            placeholder="请输入关键词"
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

    <!-- 表格与操作 -->
    <a-card :bordered="false" style="margin-top: 16px;">
      <div class="table-operations">
        <a-button
          v-permission="'system:post:create'"
          type="primary"
          @click="handleOpenModal(null)"
        >
          <template #icon><PlusOutlined /></template>
          新增岗位
        </a-button>
      </div>

      <a-table
        :columns="columns"
        :data-source="postList"
        :loading="loading"
        row-key="id"
        :pagination="false"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'code'">
            <a-tag color="blue">{{ record.code }}</a-tag>
          </template>

          <template v-else-if="column.key === 'status'">
            <a-tag :color="record.status ? 'success' : 'error'">
              {{ record.status ? '正常' : '停用' }}
            </a-tag>
          </template>

          <template v-else-if="column.key === 'action'">
            <a-space>
              <a-button
                v-permission="'system:post:update'"
                type="link"
                size="small"
                @click="handleOpenModal(record)"
              >
                编辑
              </a-button>
              <a-popconfirm
                title="确定删除此岗位吗？"
                ok-text="确认"
                cancel-text="取消"
                @confirm="handleDelete(record.id)"
              >
                <a-button
                  v-permission="'system:post:delete'"
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

    <!-- 新增 / 编辑岗位弹窗 -->
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
        <a-form-item label="岗位名称" name="name">
          <a-input v-model:value="formState.name" placeholder="请输入岗位名称 (如: 架构师)" />
        </a-form-item>

        <a-form-item label="岗位编码" name="code">
          <a-input v-model:value="formState.code" placeholder="请输入岗位编码 (如: ARCHITECT)" />
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
import { ref, reactive, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import { PlusOutlined, SearchOutlined, ReloadOutlined } from '@ant-design/icons-vue'
import { getPostList, createPost, updatePost, deletePost } from '@/api/post'

const loading = ref(false)
const postList = ref([])

const queryParam = reactive({
  search: ''
})

const columns = [
  { title: '岗位名称', dataIndex: 'name', key: 'name', width: 220 },
  { title: '岗位编码', dataIndex: 'code', key: 'code', width: 200 },
  { title: '显示排序', dataIndex: 'sort', key: 'sort', width: 120, align: 'center' },
  { title: '状态', dataIndex: 'status', key: 'status', width: 120, align: 'center' },
  { title: '操作', key: 'action', width: 180, align: 'center' }
]

const modalVisible = ref(false)
const modalTitle = ref('新增岗位')
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
  name: [{ required: true, message: '请输入岗位名称', trigger: 'blur' }],
  code: [{ required: true, message: '请输入岗位编码', trigger: 'blur' }]
}

const fetchData = async () => {
  try {
    loading.value = true
    const res = await getPostList(queryParam)
    postList.value = res || []
  } catch (error) {
    message.error('获取岗位列表失败')
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
    modalTitle.value = '编辑岗位'
    editingId.value = record.id
    formState.name = record.name
    formState.code = record.code
    formState.sort = record.sort
    formState.status = record.status
    formState.version = record.version || 1
  } else {
    modalTitle.value = '新增岗位'
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
      await updatePost(editingId.value, { ...formState })
      message.success('修改岗位成功')
    } else {
      await createPost({ ...formState })
      message.success('新增岗位成功')
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
    await deletePost(id)
    message.success('删除岗位成功')
    await fetchData()
  } catch (error) {
    message.error(error.message || '删除岗位失败')
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

