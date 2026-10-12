/**
 * 将扁平数组构造为树状结构
 * @param {Array} list 扁平数组
 * @param {string} idKey 唯一主键
 * @param {string} parentIdKey 父级ID字段
 * @param {string} childrenKey 子节点字段
 * @returns {Array} 树状数组
 */
export function buildTree(list = [], idKey = 'id', parentIdKey = 'parentId', childrenKey = 'children') {
  const map = {}
  const tree = []

  list.forEach(item => {
    map[item[idKey]] = { ...item, [childrenKey]: [] }
  })

  list.forEach(item => {
    const parentId = item[parentIdKey]
    if (parentId && map[parentId]) {
      map[parentId][childrenKey].push(map[item[idKey]])
    } else {
      tree.push(map[item[idKey]])
    }
  })

  return tree
}

/**
 * 遍历并展平树
 * @param {Array} tree 
 * @param {string} childrenKey 
 * @returns {Array}
 */
export function flattenTree(tree = [], childrenKey = 'children') {
  const result = []
  function traverse(nodes) {
    if (!Array.isArray(nodes)) return
    nodes.forEach(node => {
      const copy = { ...node }
      delete copy[childrenKey]
      result.push(copy)
      if (node[childrenKey] && node[childrenKey].length) {
        traverse(node[childrenKey])
      }
    })
  }
  traverse(tree)
  return result
}

/**
 * 在树中递归查找节点
 * @param {Array} tree 
 * @param {Function} predicate 
 * @param {string} childrenKey 
 * @returns {object|null}
 */
export function findNode(tree = [], predicate, childrenKey = 'children') {
  for (const node of tree) {
    if (predicate(node)) return node
    if (node[childrenKey] && node[childrenKey].length) {
      const found = findNode(node[childrenKey], predicate, childrenKey)
      if (found) return found
    }
  }
  return null
}

/**
 * 递归过滤树
 * @param {Array} tree 
 * @param {Function} predicate 
 * @param {string} childrenKey 
 * @returns {Array}
 */
export function filterTree(tree = [], predicate, childrenKey = 'children') {
  return tree
    .filter(node => predicate(node))
    .map(node => {
      const children = node[childrenKey] ? filterTree(node[childrenKey], predicate, childrenKey) : []
      return {
        ...node,
        [childrenKey]: children
      }
    })
}

