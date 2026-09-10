<script setup>
import { ref } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

const props = defineProps({
  project: { type: Object, required: true },
});
defineEmits(["viewDetails"]);

const imageBroken = ref(false);

function formatDate(iso) {
  if (!iso) return "—";
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return String(iso).slice(0, 10);
  return d.toLocaleDateString(undefined, { year: "numeric", month: "short", day: "numeric" });
}

function getStatusSeverity(status) {
  const s = String(status).toLowerCase();
  if (s.includes("going") || s.includes("active") || s.includes("progreso")) return "success";
  if (s.includes("plan")) return "info";
  if (s.includes("complete")) return "secondary";
  if (s.includes("hold") || s.includes("pausa")) return "warning";
  return "info";
}
</script>

<template>
  <div class="project-card-box">
    <!-- Header Image / Banner -->
    <div class="card-media">
      <img
        v-if="project.imageUrl && !imageBroken"
        :src="project.imageUrl"
        :alt="project.name"
        class="card-media__img"
        @error="imageBroken = true"
        loading="lazy"
      />
      <div v-else class="card-media__placeholder">
        <i class="pi pi-building"></i>
      </div>
      <div class="card-media__status">
        <pv-tag
          :value="project.statusLabel"
          :severity="getStatusSeverity(project.statusLabel)"
          class="status-tag"
        />
      </div>
    </div>

    <!-- Card Content -->
    <div class="card-body">
      <div class="card-header">
        <h3 class="card-title" :title="project.name">{{ project.name }}</h3>
        <p v-if="project.location" class="card-location">
          <i class="pi pi-map-marker"></i>
          <span>{{ project.location }}</span>
        </p>
      </div>

      <!-- Stats: Units & Occupancy -->
      <div class="card-stats">
        <div class="stat-item">
          <span class="stat-label">{{ t("projects.fields.total-units") }}</span>
          <span class="stat-val">{{ project.totalUnits }}</span>
        </div>
        <div class="stat-item">
          <span class="stat-label">{{ t("projects.fields.occupied-units") }}</span>
          <span class="stat-val">{{ project.occupiedUnits }}</span>
        </div>
      </div>

      <!-- Progress bar -->
      <div class="occupancy-bar-wrap">
        <div class="occupancy-bar-label">
          <span>{{ t("projects.fields.occupancy-rate") }}</span>
          <span class="font-semibold">{{ project.occupancyRate }}%</span>
        </div>
        <div class="occupancy-track">
          <div
            class="occupancy-fill"
            :style="{ width: `${project.occupancyRate}%` }"
          ></div>
        </div>
      </div>

      <!-- Footer with Date & Action -->
      <div class="card-footer">
        <span class="card-date" v-if="project.createdDate">
          <i class="pi pi-calendar"></i>
          {{ formatDate(project.createdDate) }}
        </span>
        <pv-button
          :label="t('projects.actions.view-details')"
          icon="pi pi-arrow-right"
          iconPos="right"
          size="small"
          class="custom-green-button w-full mt-2"
          @click="$emit('viewDetails')"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.project-card-box {
  background: #ffffff;
  border-radius: 0.75rem;
  border: 1px solid #e5e7eb;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.06);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.project-card-box:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.08);
}

.card-media {
  position: relative;
  width: 100%;
  aspect-ratio: 16 / 9;
  background: #f3f4f6;
  overflow: hidden;
}

.card-media__img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.card-media__placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #10b981, #047857);
}

.card-media__placeholder .pi {
  font-size: 2.5rem;
  color: #ffffff;
  opacity: 0.9;
}

.card-media__status {
  position: absolute;
  top: 0.6rem;
  right: 0.6rem;
}

.status-tag {
  font-size: 0.75rem !important;
  font-weight: 600 !important;
  padding: 0.2rem 0.6rem !important;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.card-body {
  padding: 1.1rem;
  display: flex;
  flex-direction: column;
  flex: 1;
  gap: 0.85rem;
}

.card-header {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.card-title {
  font-size: 1.05rem;
  font-weight: 700;
  color: #111827;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.card-location {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.8rem;
  color: #6b7280;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.card-location .pi {
  color: #10b981;
  font-size: 0.8rem;
}

.card-stats {
  display: flex;
  justify-content: space-between;
  padding: 0.5rem 0.75rem;
  background: #f9fafb;
  border-radius: 0.5rem;
  border: 1px solid #f3f4f6;
}

.stat-item {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 0.72rem;
  color: #6b7280;
  font-weight: 500;
}

.stat-val {
  font-size: 1.1rem;
  font-weight: 700;
  color: #111827;
}

.occupancy-bar-wrap {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
}

.occupancy-bar-label {
  display: flex;
  justify-content: space-between;
  font-size: 0.75rem;
  color: #4b5563;
}

.occupancy-track {
  width: 100%;
  height: 6px;
  background: #e5e7eb;
  border-radius: 9999px;
  overflow: hidden;
}

.occupancy-fill {
  height: 100%;
  background: linear-gradient(90deg, #10b981, #059669);
  border-radius: 9999px;
  transition: width 0.3s ease;
}

.card-footer {
  margin-top: auto;
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  padding-top: 0.5rem;
  border-top: 1px solid #f3f4f6;
}

.card-date {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  font-size: 0.75rem;
  color: #9ca3af;
}

.card-date .pi {
  font-size: 0.72rem;
}

:deep(.custom-green-button) {
  background-color: #10b981 !important;
  border-color: #10b981 !important;
  color: white !important;
  font-weight: 600 !important;
  font-size: 0.85rem !important;
  padding: 0.45rem 0.85rem !important;
  transition: all 0.2s ease !important;
}

:deep(.custom-green-button:hover) {
  background-color: #059669 !important;
  border-color: #059669 !important;
}

:deep(.custom-green-button:focus) {
  box-shadow: 0 0 0 0.2rem rgba(16, 185, 129, 0.4) !important;
}
</style>
