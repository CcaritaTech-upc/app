<script setup>
import { computed, reactive, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { DeviceStatus } from '../../domain/model/device-status.enum.js';
import { isValidName, isValidMacAddress } from '../../../shared/presentation/validators.js';

const props = defineProps({
  modelValue: { type: Boolean, default: false },
  device: { type: Object, default: null },
  isNew: { type: Boolean, default: false }
});

const emit = defineEmits(['update:modelValue', 'save', 'cancel']);
const { t } = useI18n();

const form = reactive({ id: null, name: '', type: '', location: '', projectId: null, status: DeviceStatus.ONLINE, macAddress: '' });

const errors = reactive({
  name: '',
  type: '',
  location: '',
  macAddress: ''
});

const typeOptions = [
  { label: t('devices.types.temperature'), value: 'temperature' },
  { label: t('devices.types.energy'), value: 'energy' },
  { label: t('devices.types.humidity'), value: 'humidity' },
  { label: t('devices.types.door'), value: 'door' },
  { label: t('devices.types.water'), value: 'water' },
  { label: t('devices.types.security'), value: 'security' },
  { label: t('devices.types.lighting'), value: 'lighting' },
  { label: t('devices.types.hvac'), value: 'hvac' }
];

const statusOptions = [
  { label: t('devices.status.online'), value: DeviceStatus.ONLINE },
  { label: t('devices.status.offline'), value: DeviceStatus.OFFLINE }
];

watch(() => props.device, (d) => {
  if (d) {
    form.id = d.id;
    form.name = d.name;
    form.type = d.type;
    form.location = d.location;
    form.projectId = d.projectId;
    form.status = d.status || DeviceStatus.ONLINE;
    form.macAddress = d.macAddress || '';
  }
  errors.name = '';
  errors.type = '';
  errors.location = '';
  errors.macAddress = '';
}, { immediate: true });

watch(() => props.modelValue, (isVisible) => {
  if (!isVisible) {
    form.id = null;
    form.name = '';
    form.type = '';
    form.location = '';
    form.projectId = null;
    form.status = DeviceStatus.ONLINE;
    form.macAddress = '';
  }
  errors.name = '';
  errors.type = '';
  errors.location = '';
  errors.macAddress = '';
});

const visible = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val)
});

function validate() {
  errors.name = '';
  errors.type = '';
  errors.location = '';
  errors.macAddress = '';
  let valid = true;

  if (!isValidName(form.name, 2, 50)) {
    errors.name = 'El nombre del dispositivo debe tener entre 2 y 50 caracteres.';
    valid = false;
  }
  if (!form.type) {
    errors.type = 'Seleccione un tipo de dispositivo.';
    valid = false;
  }
  if (!form.location || form.location.trim().length < 2 || form.location.trim().length > 100) {
    errors.location = 'La ubicación debe tener entre 2 y 100 caracteres.';
    valid = false;
  }
  if (form.macAddress && !isValidMacAddress(form.macAddress)) {
    errors.macAddress = 'Formato de dirección MAC inválido (ej: AA:BB:CC:DD:EE:FF).';
    valid = false;
  }
  return valid;
}

const onHide = () => {
  emit('cancel');
};

const onSave = () => {
  if (!validate()) return;
  emit('save', {
    ...form,
    name: form.name.trim(),
    location: form.location.trim(),
    macAddress: form.macAddress ? form.macAddress.trim() : ''
  });
};
</script>

<template>
  <pv-dialog v-model:visible="visible" modal :header="props.isNew ? t('devices.add.title') : t('devices.edit.title')" :style="{ width: '450px' }" @hide="onHide" contentClass="device-edit-dialog">
    <div class="flex flex-column gap-4 dialog-body">
      <div class="flex flex-column gap-2">
        <label class="font-medium text-gray-800">{{ t('devices.fields.name') }} *</label>
        <pv-input-text
          v-model="form.name"
          :placeholder="t('devices.fields.name')"
          :invalid="!!errors.name"
          class="text-input"
          @input="errors.name = ''"
        />
        <small v-if="errors.name" class="p-error">{{ errors.name }}</small>
      </div>

      <div class="flex flex-column gap-2">
        <label class="font-medium text-gray-800">{{ t('devices.fields.type') }} *</label>
        <pv-select
          v-model="form.type"
          :options="typeOptions"
          optionLabel="label"
          optionValue="value"
          :placeholder="t('devices.fields.type')"
          :invalid="!!errors.type"
          class="select-input device-select-darktext"
          @change="errors.type = ''"
        />
        <small v-if="errors.type" class="p-error">{{ errors.type }}</small>
      </div>

      <div class="flex flex-column gap-2">
        <label class="font-medium text-gray-800">{{ t('devices.fields.location') }} *</label>
        <pv-input-text
          v-model="form.location"
          :placeholder="t('devices.fields.location')"
          :invalid="!!errors.location"
          class="text-input"
          @input="errors.location = ''"
        />
        <small v-if="errors.location" class="p-error">{{ errors.location }}</small>
      </div>

      <div class="flex flex-column gap-2">
        <label class="font-medium text-gray-800">{{ t('devices.fields.macAddress') }}</label>
        <pv-input-text
          v-model="form.macAddress"
          :placeholder="t('devices.fields.macAddress')"
          :invalid="!!errors.macAddress"
          class="text-input"
          @input="errors.macAddress = ''"
        />
        <small v-if="errors.macAddress" class="p-error">{{ errors.macAddress }}</small>
      </div>

      <div v-if="!props.isNew" class="flex flex-column gap-2">
        <label class="font-medium text-gray-800">{{ t('devices.fields.status') }}</label>
        <pv-select v-model="form.status" :options="statusOptions" optionLabel="label" optionValue="value" class="select-input device-select-darktext" />
      </div>
    </div>

    <template #footer>
      <div class="flex justify-content-end gap-2 w-full">
        <pv-button :label="t('devices.actions.cancel')" text @click="visible = false" />
        <pv-button :label="t('devices.actions.save')" @click="onSave" />
      </div>
    </template>
  </pv-dialog>
</template>

<style scoped>
.flex-column { display: flex; flex-direction: column; }

/* Fondo blanco y texto oscuro para asegurar contraste */
:deep(.device-edit-dialog) {
  background: #ffffff !important;
  color: #111827 !important;
}
:deep(.device-edit-dialog .p-dialog-header) {
  background: #ffffff !important;
  color: #111827 !important;
  border-bottom: 1px solid #e5e7eb !important;
}
:deep(.device-edit-dialog .p-dialog-content) {
  background: #ffffff !important;
  color: #111827 !important;
}
:deep(.device-edit-dialog .p-dialog-footer) {
  background: #ffffff !important;
  border-top: 1px solid #e5e7eb !important;
}

/* Labels */
:deep(.device-edit-dialog label) {
  color: #374151 !important;
}

/* Inputs de texto */
:deep(.device-edit-dialog .p-inputtext) {
  background: #ffffff !important;
  color: #111827 !important;
  border: 1px solid #d1d5db !important;
}
:deep(.device-edit-dialog .p-inputtext::placeholder) {
  color: #6b7280 !important;
  opacity: 1 !important;
}
:deep(.device-edit-dialog .p-inputtext:focus) {
  border-color: #3b82f6 !important;
  box-shadow: 0 0 0 1px #3b82f6 !important;
}

/* Select (PrimeVue Select / Dropdown) */
:deep(.device-edit-dialog .p-select) {
  background: #ffffff !important;
  color: #111827 !important;
  border: 1px solid #d1d5db !important;
}
:deep(.device-edit-dialog .p-select:hover) {
  border-color: #9ca3af !important;
}
:deep(.device-edit-dialog .p-select:focus-within) {
  border-color: #3b82f6 !important;
  box-shadow: 0 0 0 1px #3b82f6 !important;
}

/* Refuerzos de estilo para Select (alineado con client-edit-dialog) */
:deep(.device-edit-dialog .p-select .p-select-label) {
  color: #111827 !important;
  background: #ffffff !important;
}
:deep(.device-edit-dialog .p-select .p-select-label.p-placeholder) {
  color: #6b7280 !important;
}
:deep(.device-edit-dialog .p-select:enabled:hover) {
  border-color: #9ca3af !important;
}
:deep(.device-edit-dialog .p-select.p-focus),
:deep(.device-edit-dialog .p-select:focus),
:deep(.device-edit-dialog .p-select:focus-within) {
  border-color: #3b82f6 !important;
  box-shadow: 0 0 0 0.2rem rgba(59,130,246,0.25) !important;
}
:deep(.device-edit-dialog .p-select-panel) {
  background: #ffffff !important;
  color: #111827 !important;
  border: 1px solid #d1d5db !important;
}
:deep(.device-edit-dialog .p-select-panel .p-select-option) {
  background: #ffffff !important;
  color: #111827 !important;
}
:deep(.device-edit-dialog .p-select-panel .p-select-option:hover) {
  background: #f3f4f6 !important;
  color: #111827 !important;
}
:deep(.device-edit-dialog .p-select-panel .p-select-option.p-highlight) {
  background: #3b82f6 !important;
  color: #ffffff !important;
}
/* Ícono del trigger */
:deep(.device-edit-dialog .p-select-trigger .p-icon) {
  color: #374151 !important;
}
:deep(.device-edit-dialog .p-dialog-header .p-dialog-header-icon) {
  color: #374151 !important;
}
:deep(.device-edit-dialog .p-dialog-header .p-dialog-header-icon:hover) {
  background: #f3f4f6 !important;
}

:deep(.device-edit-dialog) {
  --p-select-color: #111827 !important; /* variable consumida por .p-select-label */
  --p-select-placeholder-color: #6b7280 !important;
}
:deep(.device-edit-dialog .p-select-label) {
  color: #111827 !important; /* fuerza directo si el theme ignora la variable */
}
:deep(.device-edit-dialog .p-select-label.p-placeholder) {
  color: #6b7280 !important;
}
</style>
