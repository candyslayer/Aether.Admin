CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_tenant (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        code character varying(64) NOT NULL,
        status boolean NOT NULL,
        expire_time timestamp with time zone,
        permission_version bigint NOT NULL DEFAULT 1,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        CONSTRAINT "PK_sys_tenant" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_audit_log (
        id uuid NOT NULL,
        user_id uuid,
        method character varying(16) NOT NULL,
        endpoint character varying(512) NOT NULL,
        parameters jsonb NOT NULL,
        ip character varying(64) NOT NULL,
        trace_id character varying(64) NOT NULL,
        result integer NOT NULL,
        duration_ms bigint NOT NULL,
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        version bigint NOT NULL DEFAULT 1,
        CONSTRAINT "PK_sys_audit_log" PRIMARY KEY (id),
        CONSTRAINT "AK_sys_audit_log_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT "FK_sys_audit_log_sys_tenant_tenant_id" FOREIGN KEY (tenant_id) REFERENCES sys_tenant (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_menu (
        id uuid NOT NULL,
        parent_id uuid,
        name character varying(100) NOT NULL,
        type integer NOT NULL,
        path character varying(256),
        component character varying(256),
        permission_code character varying(128),
        sort integer NOT NULL,
        visible boolean NOT NULL,
        status boolean NOT NULL,
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        version bigint NOT NULL DEFAULT 1,
        CONSTRAINT "PK_sys_menu" PRIMARY KEY (id),
        CONSTRAINT "AK_sys_menu_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT "FK_sys_menu_sys_menu_tenant_id_parent_id" FOREIGN KEY (tenant_id, parent_id) REFERENCES sys_menu (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_sys_menu_sys_tenant_tenant_id" FOREIGN KEY (tenant_id) REFERENCES sys_tenant (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_organization (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        code character varying(64) NOT NULL,
        status boolean NOT NULL,
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        version bigint NOT NULL DEFAULT 1,
        CONSTRAINT "PK_sys_organization" PRIMARY KEY (id),
        CONSTRAINT "AK_sys_organization_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT "FK_sys_organization_sys_tenant_tenant_id" FOREIGN KEY (tenant_id) REFERENCES sys_tenant (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_post (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        code character varying(64) NOT NULL,
        sort integer NOT NULL,
        status boolean NOT NULL,
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        version bigint NOT NULL DEFAULT 1,
        CONSTRAINT "PK_sys_post" PRIMARY KEY (id),
        CONSTRAINT "AK_sys_post_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT "FK_sys_post_sys_tenant_tenant_id" FOREIGN KEY (tenant_id) REFERENCES sys_tenant (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_role (
        id uuid NOT NULL,
        name character varying(100) NOT NULL,
        code character varying(64) NOT NULL,
        type integer NOT NULL,
        status boolean NOT NULL,
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        version bigint NOT NULL DEFAULT 1,
        CONSTRAINT "PK_sys_role" PRIMARY KEY (id),
        CONSTRAINT "AK_sys_role_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT "FK_sys_role_sys_tenant_tenant_id" FOREIGN KEY (tenant_id) REFERENCES sys_tenant (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_data_scope (
        id uuid NOT NULL,
        role_id uuid NOT NULL,
        scope_type integer NOT NULL,
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        version bigint NOT NULL DEFAULT 1,
        CONSTRAINT "PK_sys_data_scope" PRIMARY KEY (id),
        CONSTRAINT "AK_sys_data_scope_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT "FK_sys_data_scope_sys_role_tenant_id_role_id" FOREIGN KEY (tenant_id, role_id) REFERENCES sys_role (tenant_id, id) ON DELETE CASCADE,
        CONSTRAINT "FK_sys_data_scope_sys_tenant_tenant_id" FOREIGN KEY (tenant_id) REFERENCES sys_tenant (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_role_menus (
        tenant_id uuid NOT NULL,
        role_id uuid NOT NULL,
        menu_id uuid NOT NULL,
        CONSTRAINT "PK_sys_role_menus" PRIMARY KEY (tenant_id, role_id, menu_id),
        CONSTRAINT "FK_sys_role_menus_sys_menu_tenant_id_menu_id" FOREIGN KEY (tenant_id, menu_id) REFERENCES sys_menu (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_sys_role_menus_sys_role_tenant_id_role_id" FOREIGN KEY (tenant_id, role_id) REFERENCES sys_role (tenant_id, id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_dept (
        id uuid NOT NULL,
        organization_id uuid NOT NULL,
        parent_id uuid,
        name character varying(100) NOT NULL,
        tree_path character varying(4000) NOT NULL,
        leader_id uuid,
        sort integer NOT NULL,
        status boolean NOT NULL,
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        version bigint NOT NULL DEFAULT 1,
        CONSTRAINT "PK_sys_dept" PRIMARY KEY (id),
        CONSTRAINT "AK_sys_dept_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT "AK_sys_dept_tenant_id_organization_id_id" UNIQUE (tenant_id, organization_id, id),
        CONSTRAINT "FK_sys_dept_sys_dept_tenant_id_organization_id_parent_id" FOREIGN KEY (tenant_id, organization_id, parent_id) REFERENCES sys_dept (tenant_id, organization_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_sys_dept_sys_organization_tenant_id_organization_id" FOREIGN KEY (tenant_id, organization_id) REFERENCES sys_organization (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_sys_dept_sys_tenant_tenant_id" FOREIGN KEY (tenant_id) REFERENCES sys_tenant (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_role_depts (
        tenant_id uuid NOT NULL,
        role_id uuid NOT NULL,
        dept_id uuid NOT NULL,
        CONSTRAINT "PK_sys_role_depts" PRIMARY KEY (tenant_id, role_id, dept_id),
        CONSTRAINT "FK_sys_role_depts_sys_dept_tenant_id_dept_id" FOREIGN KEY (tenant_id, dept_id) REFERENCES sys_dept (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_sys_role_depts_sys_role_tenant_id_role_id" FOREIGN KEY (tenant_id, role_id) REFERENCES sys_role (tenant_id, id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_user (
        id uuid NOT NULL,
        dept_id uuid NOT NULL,
        username character varying(64) NOT NULL,
        password_hash character varying(256) NOT NULL,
        nickname character varying(100) NOT NULL,
        email character varying(254),
        mobile character varying(32),
        status boolean NOT NULL,
        security_stamp uuid NOT NULL,
        tenant_id uuid NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (now()),
        updated_at timestamp with time zone NOT NULL DEFAULT (now()),
        version bigint NOT NULL DEFAULT 1,
        CONSTRAINT "PK_sys_user" PRIMARY KEY (id),
        CONSTRAINT "AK_sys_user_tenant_id_id" UNIQUE (tenant_id, id),
        CONSTRAINT "FK_sys_user_sys_dept_tenant_id_dept_id" FOREIGN KEY (tenant_id, dept_id) REFERENCES sys_dept (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_sys_user_sys_tenant_tenant_id" FOREIGN KEY (tenant_id) REFERENCES sys_tenant (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_user_depts (
        tenant_id uuid NOT NULL,
        user_id uuid NOT NULL,
        dept_id uuid NOT NULL,
        CONSTRAINT "PK_sys_user_depts" PRIMARY KEY (tenant_id, user_id, dept_id),
        CONSTRAINT "FK_sys_user_depts_sys_dept_tenant_id_dept_id" FOREIGN KEY (tenant_id, dept_id) REFERENCES sys_dept (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_sys_user_depts_sys_user_tenant_id_user_id" FOREIGN KEY (tenant_id, user_id) REFERENCES sys_user (tenant_id, id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_user_posts (
        tenant_id uuid NOT NULL,
        user_id uuid NOT NULL,
        post_id uuid NOT NULL,
        CONSTRAINT "PK_sys_user_posts" PRIMARY KEY (tenant_id, user_id, post_id),
        CONSTRAINT "FK_sys_user_posts_sys_post_tenant_id_post_id" FOREIGN KEY (tenant_id, post_id) REFERENCES sys_post (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_sys_user_posts_sys_user_tenant_id_user_id" FOREIGN KEY (tenant_id, user_id) REFERENCES sys_user (tenant_id, id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE TABLE sys_user_roles (
        tenant_id uuid NOT NULL,
        user_id uuid NOT NULL,
        role_id uuid NOT NULL,
        CONSTRAINT "PK_sys_user_roles" PRIMARY KEY (tenant_id, user_id, role_id),
        CONSTRAINT "FK_sys_user_roles_sys_role_tenant_id_role_id" FOREIGN KEY (tenant_id, role_id) REFERENCES sys_role (tenant_id, id) ON DELETE RESTRICT,
        CONSTRAINT "FK_sys_user_roles_sys_user_tenant_id_user_id" FOREIGN KEY (tenant_id, user_id) REFERENCES sys_user (tenant_id, id) ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE INDEX "IX_sys_audit_log_tenant_id_created_at" ON sys_audit_log (tenant_id, created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_sys_data_scope_tenant_id_role_id" ON sys_data_scope (tenant_id, role_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE INDEX "IX_sys_dept_tenant_id_leader_id" ON sys_dept (tenant_id, leader_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE INDEX "IX_sys_dept_tenant_id_organization_id_parent_id" ON sys_dept (tenant_id, organization_id, parent_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE INDEX "IX_sys_dept_tenant_id_parent_id" ON sys_dept (tenant_id, parent_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE INDEX "IX_sys_menu_tenant_id_parent_id" ON sys_menu (tenant_id, parent_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_sys_organization_tenant_id_code" ON sys_organization (tenant_id, code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_sys_post_tenant_id_code" ON sys_post (tenant_id, code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_sys_role_tenant_id_code" ON sys_role (tenant_id, code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE INDEX "IX_sys_role_depts_tenant_id_dept_id" ON sys_role_depts (tenant_id, dept_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE INDEX "IX_sys_role_menus_tenant_id_menu_id" ON sys_role_menus (tenant_id, menu_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_sys_tenant_code" ON sys_tenant (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE INDEX "IX_sys_user_tenant_id_dept_id" ON sys_user (tenant_id, dept_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE UNIQUE INDEX "IX_sys_user_tenant_id_username" ON sys_user (tenant_id, username);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE INDEX "IX_sys_user_depts_tenant_id_dept_id" ON sys_user_depts (tenant_id, dept_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE INDEX "IX_sys_user_posts_tenant_id_post_id" ON sys_user_posts (tenant_id, post_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    CREATE INDEX "IX_sys_user_roles_tenant_id_role_id" ON sys_user_roles (tenant_id, role_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    ALTER TABLE sys_dept ADD CONSTRAINT "FK_sys_dept_sys_user_tenant_id_leader_id" FOREIGN KEY (tenant_id, leader_id) REFERENCES sys_user (tenant_id, id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20261010031658_InitialSchema') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20261010031658_InitialSchema', '10.0.12');
    END IF;
END $EF$;
COMMIT;

