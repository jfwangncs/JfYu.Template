<script lang="ts" setup>
import type { SystemPermissionApi } from '#/api/system/permission';

import type {
  OnActionClickParams,
  VxeTableGridOptions,
} from '#/adapter/vxe-table';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { RotateCw } from '@vben/icons';

import { Button, message } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { getPermissionList, syncPermissions } from '#/api/system/permission';
import { $t } from '#/locales';

import { useColumns, useGridFormSchema } from './data';
import Form from './modules/form.vue';

const [FormDrawer, formDrawerApi] = useVbenDrawer({
  connectedComponent: Form,
  destroyOnClose: true,
});

const [Grid, gridApi] = useVbenVxeGrid({
  gridOptions: {
    columns: useColumns(onActionClick),
    height: 'auto',
    keepSource: true,
    proxyConfig: {
      ajax: {
        query: async ({ page }, formValues) => {
          return await getPermissionList({
            pageIndex: page.currentPage,
            pageSize: page.pageSize,
            ...formValues,
          });
        },
      },
    },
    rowConfig: {
      keyField: 'id',
    },
    toolbarConfig: {
      custom: true,
      export: false,
      refresh: true,
      search: true,
      zoom: false,
    },
  } as VxeTableGridOptions<SystemPermissionApi.SystemPermission>,
  formOptions: {
    schema: useGridFormSchema(),
    submitOnChange: true,
  },
});

function onActionClick(
  e: OnActionClickParams<SystemPermissionApi.SystemPermission>,
) {
  if (e.code === 'edit') {
    formDrawerApi.setData(e.row).open();
  }
}

function onRefresh() {
  gridApi.query();
}

async function onSync() {
  await syncPermissions();
  message.success($t('system.permission.syncSuccess'));
  gridApi.query();
}
</script>

<template>
  <Page auto-content-height>
    <FormDrawer @success="onRefresh" />
    <Grid>
      <template #toolbar-tools>
        <Button type="primary" @click="onSync">
          <RotateCw class="mr-1 size-4" />
          {{ $t('system.permission.sync') }}
        </Button>
      </template>
    </Grid>
  </Page>
</template>
