<template>
  <div class="audit-management">
    <!-- 检索过滤卡片 -->
    <a-card :bordered="false" class="search-card">
      <a-form layout="inline" :model="queryParam">
        <a-form-item label="请求端点 (Endpoint)">
          <a-input
            v-model:value="queryParam.endpoint"
            placeholder="如: /api/users"
            allow-clear
            style="width: 200px"
            @pressEnter="handleSearch"
          />
        </a-form-item>

        <a-form-item label="请求方法 (Method)">
          <a-select
            v-model:value="queryParam.method"
            placeholder="全部方法"
            allow-clear
            style="width: 130px"
          >
            <a-select-option value="GET">GET</a-select-option>
            <a-select-option value="POST">POST</a-select-option>
            <a-select-option value="PUT">PUT</a-select-option>
            <a-select-option value="DELETE">DELETE</a-select-option>
          </a-select>
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

    <!-- 审计日志数据表格 -->
    <a-card :bordered="false" style="margin-top: 16px;">
      <a-table
        :columns="columns"
        :data-source="auditList"
        :loading="loading"
        :pagination="pagination"
        row-key="id"
        @change="handleTableChange"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'method'">
            <a-tag :color="getMethodColor(record.method)">
              {{ record.method }}
            </a-tag>
          </template>

          <template v-else-if="column.key === 'endpoint'">
            <span style="font-family: monospace; font-weight: 500;">{{ record.endpoint }}</span>
          </template>

          <template v-else-if="column.key === 'result'">
            <a-badge
              :status="record.result < 400 ? 'success' : 'error'"
              :text="`${record.result} HTTP`"
            />
          </template>

          <template v-else-if="column.key === 'durationMs'">
            <a-tag :color="record.durationMs > 200 ? 'orange' : 'green'">
              {{ formatDuration(record.durationMs) }}
            </a-tag>
          </template>

          <template v-else-if="column.key === 'createdAt'">
            <span>{{ formatDate(record.createdAt) }}</span>
          </template>

          <template v-else-if="column.key === 'action'">
            <a-button type="link" size="small" @click="handleViewDetail(record)">
              详情
            </a-button>
          </template>
        </template>
      </a-table>
    </a-card>

    <!-- 日志明细抽屉 Drawer -->
    <a-drawer
      v-model:open="drawerVisible"
      title="操作日志与安全审计明细"
      width="600px"
    >
      <div v-if="detailRecord">
        <a-descriptions title="请求概况" :column="2" bordered size="small">
          <a-descriptions-item label="请求方式">
            <a-tag :color="getMethodColor(detailRecord.method)">{{ detailRecord.method }}</a-tag>
          </a-descriptions-item>
          <a-descriptions-item label="执行耗时">
            {{ formatDuration(detailRecord.durationMs) }}
          </a-descriptions-item>
          <a-descriptions-item label="请求端点" :span="2">
            <code>{{ detailRecord.endpoint }}</code>
          </a-descriptions-item>
          <a-descriptions-item label="操作 IP">
            {{ detailRecord.ip }}
          </a-descriptions-item>
          <a-descriptions-item label="执行结果">
            <a-tag :color="detailRecord.result < 400 ? 'success' : 'error'">
              HTTP {{ detailRecord.result }}
            </a-tag>
          </a-descriptions-item>
          <a-descriptions-item label="分布式链路 TraceId" :span="2">
            <code>{{ detailRecord.traceId }}</code>
          </a-descriptions-item>
          <a-descriptions-item label="记录时间" :span="2">
            {{ formatDate(detailRecord.createdAt) }}
          </a-descriptions-item>
        </a-descriptions>

        <div style="margin-top: 24px;">
          <h4 style="font-weight: 600; margin-bottom: 8px;">请求参数 / Payload (JSON)</h4>
          <pre class="json-code-box">{{ formatJson(detailRecord.parameters) }}</pre>
        </div>
      </div>
    </a-drawer>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { message } from 'ant-design-vue'
import { SearchOutlined, ReloadOutlined } from '@ant-design/icons-vue'
import { getAuditLogs } from '@/api/audit'
import { formatDate, formatDuration } from '@/utils/format'

const loading = ref(false)
const auditList = ref([])

const queryParam = reactive({
  endpoint: '',
  method: undefined
})

const pagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showTotal: (total) => `共 ${total} 条审计日志`
})

const columns = [
  { title: '请求方法', dataIndex: 'method', key: 'method', width: 100, align: 'center' },
  { title: 'API 端点', dataIndex: 'endpoint', key: 'endpoint', width: 220 },
  { title: '客户端 IP', dataIndex: 'ip', key: 'ip', width: 140 },
  { title: '响应状态', dataIndex: 'result', key: 'result', width: 120, align: 'center' },
  { title: '耗时', dataIndex: 'durationMs', key: 'durationMs', width: 100, align: 'center' },
  { title: '记录时间', dataIndex: 'createdAt', key: 'createdAt', width: 180 },
  { title: '操作', key: 'action', width: 90, align: 'center' }
]

const drawerVisible = ref(false)
const detailRecord = ref(null)

const getMethodColor = (method) => {
  const map = {
    GET: 'blue',
    POST: 'green',
    PUT: 'orange',
    DELETE: 'red'
  }
  return map[method] || 'default'
}

const formatJson = (str) => {
  if (!str) return '无请求参数'
  try {
    const obj = JSON.parse(str)
    return JSON.stringify(obj, null, 2)
  } catch {
    return str
  }
}

const fetchData = async () => {
  try {
    loading.value = true
    const res = await getAuditLogs({
      page: pagination.current,
      size: pagination.pageSize,
      endpoint: queryParam.endpoint || undefined,
      method: queryParam.method || undefined
    })

    if (res && res.items) {
      auditList.value = res.items
      pagination.total = res.total
    } else if (Array.isArray(res)) {
      auditList.value = res
      pagination.total = res.length
    }
  } catch (error) {
    message.error('加载审计日志失败')
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  pagination.current = 1
  fetchData()
}

const handleReset = () => {
  queryParam.endpoint = ''
  queryParam.method = undefined
  pagination.current = 1
  fetchData()
}

const handleTableChange = (pag) => {
  pagination.current = pag.current
  pagination.pageSize = pag.pageSize
  fetchData()
}

const handleViewDetail = (record) => {
  detailRecord.value = record
  drawerVisible.value = true
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.search-card {
  border-radius: 8px;
}

.json-code-box {
  background: #1e293b;
  color: #38bdf8;
  padding: 14px;
  border-radius: 6px;
  font-family: monospace;
  font-size: 13px;
  max-height: 360px;
  overflow: auto;
}
</style>

