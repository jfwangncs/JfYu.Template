<script lang="ts" setup>
import type { SystemPermissionApi } from '#/api/system/permission';

import { computed, nextTick, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { useVbenForm } from '#/adapter/form';
import { updatePermission } from '#/api/system/permission';
import { $t } from '#/locales';

import { useFormSchema } from '../data';

const emits = defineEmits(['success']);

const id = ref<number>();
const formData = ref<SystemPermissionApi.SystemPermission>();

const [Form, formApi] = useVbenForm({
  schema: useFormSchema(),
  showDefaultActions: false,
});

const [Drawer, drawerApi] = useVbenDrawer({
  async onConfirm() {
    const { valid } = await formApi.validate();
    if (!valid) return;
    const values = await formApi.getValues();
    drawerApi.lock();
    updatePermission(id.value!, values)
      .then(() => {
        emits('success');
        drawerApi.close();
      })
      .catch(() => {
        drawerApi.unlock();
      });
  },

  async onOpenChange(isOpen) {
    if (!isOpen) return;
    formApi.resetForm();
    const data = drawerApi.getData<SystemPermissionApi.SystemPermission>();
    if (data) {
      formData.value = data;
      id.value = data.id;
      await nextTick();
      formApi.setValues({
        name: data.name,
        description: data.description,
        icon: data.icon,
        sort: data.sort,
      });
    }
  },
});

const getDrawerTitle = computed(() =>
  $t('common.edit', $t('system.permission.name')),
);
</script>

<template>
  <Drawer :title="getDrawerTitle">
    <Form />
  </Drawer>
</template>
