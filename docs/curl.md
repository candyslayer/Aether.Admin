# HTTP verification

Run the commands in Bash with curl and jq. Set ADMIN_PASSWORD to the bootstrap password.

```bash
BASE=http://127.0.0.1:5080
TOKEN=$(curl -fsS "$BASE/api/auth/login" -H 'Content-Type: application/json' \
  -d "$(jq -n --arg password "$ADMIN_PASSWORD" '{tenantCode:"default",username:"admin",password:$password}')" | jq -r .data.accessToken)
ORG=$(curl -fsS "$BASE/api/organizations" -H "Authorization: Bearer $TOKEN" | jq -r '.data[0].id')
DEPT=$(curl -fsS "$BASE/api/depts" -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d "$(jq -n --arg org "$ORG" '{name:"Engineering",organizationId:$org}')" | jq -r .data.id)
POST=$(curl -fsS "$BASE/api/posts" -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{"name":"Engineer","code":"ENGINEER"}' | jq -r .data.id)
ROLE=$(curl -fsS "$BASE/api/roles" -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d '{"name":"Employee","code":"EMPLOYEE"}' | jq -r .data.id)
MENU=$(curl -fsS "$BASE/api/menus" -H "Authorization: Bearer $TOKEN" | jq -r '.. | objects | select(.permissionCode? == "system:user:list") | .id' | head -1)
curl -fsS -X PUT "$BASE/api/roles/$ROLE/permissions" -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d "$(jq -n --arg menu "$MENU" '{menuIds:[$menu],scopeType:5,departmentIds:[]}')"
curl -fsS "$BASE/api/users" -H "Authorization: Bearer $TOKEN" -H 'Content-Type: application/json' \
  -d "$(jq -n --arg dept "$DEPT" --arg role "$ROLE" --arg post "$POST" '{username:"employee",password:"EmployeePassword2026!",nickname:"Employee",deptId:$dept,roleIds:[$role],postIds:[$post]}')"
EMPLOYEE_TOKEN=$(curl -fsS "$BASE/api/auth/login" -H 'Content-Type: application/json' \
  -d '{"tenantCode":"default","username":"employee","password":"EmployeePassword2026!"}' | jq -r .data.accessToken)
# SELF returns only the employee.
curl -fsS "$BASE/api/users" -H "Authorization: Bearer $EMPLOYEE_TOKEN"
# Missing permission returns 403.
curl -sS -o /dev/null -w '%{http_code}\n' -X DELETE "$BASE/api/users/00000000-0000-0000-0000-000000000001" -H "Authorization: Bearer $EMPLOYEE_TOKEN"
```

The executable PowerShell test also verifies DEPT, DEPT_CHILD, CUSTOM, ALL, grant revocation, tree cycles, optimistic concurrency and audit persistence. Resource names in the curl example must be unique when rerunning.
