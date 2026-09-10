<template>
  <div class="profile-container">
    <h1 class="profile-title">{{ $t('profile.title') }}</h1>
    <div class="profile-header">
      <div class="profile-card">
        <div class="photo-wrapper">
          <img v-if="profile.photoUrl" :src="profile.photoUrl" alt="Profile Photo" class="profile-photo" />
          <div v-else class="profile-photo placeholder">
            <span>{{ profile.name ? profile.name.charAt(0) : '?' }}</span>
          </div>
          <div v-if="isEditing" class="photo-upload-overlay">
            <input
              type="file"
              ref="fileInput"
              accept="image/*"
              style="display: none;"
              @change="handlePhotoChange"
            />
            <pv-button
              type="button"
              icon="pi pi-camera"
              class="change-photo-btn"
              rounded
              @click="triggerPhotoUpload"
              title="Cambiar foto"
            />
          </div>
        </div>
        <div class="profile-info">
          <h2 class="profile-name">{{ profile.name }}</h2>
          <p class="profile-role">{{ $t('profile.ownerRole') }}</p>
        </div>
        <div class="edit-buttons">
          <pv-button v-if="!isEditing" :label="$t('profile.edit')" class="edit-button" @click="toggleEdit" />
          <div v-else class="edit-actions">
            <pv-button :label="$t('profile.save')" class="edit-button" @click="toggleEdit" />
            <pv-button :label="$t('profile.cancel')" class="cancel-button" @click="cancelEdit" />
          </div>
        </div>
      </div>
    </div>

    <div class="profile-body">
      <div class="account-card">
        <h3 class="card-title">{{ $t('profile.accountInformation') }}</h3>
        <div class="info-group">
          <label>{{ $t('profile.fullName') }}</label>
          <input type="text" v-model="profile.name" :readonly="!isEditing" class="info-input" />
        </div>
        <div class="info-group">
          <label>{{ $t('profile.email') }}</label>
          <input type="text" v-model="profile.email" readonly class="info-input" />
        </div>
        <div class="info-group">
          <label>{{ $t('profile.phoneNumber') }}</label>
          <input type="text" v-model="profile.phoneNumber" :readonly="!isEditing" class="info-input" />
        </div>
        <div class="info-group">
          <label>{{ $t('profile.address') }}</label>
          <input type="text" v-model="profile.address" :readonly="!isEditing" class="info-input" />
        </div>
        <div class="info-group">
          <label>{{ $t('profile.secondEmail') }}</label>
          <input type="email" v-model="profile.secondEmail" :readonly="!isEditing" class="info-input" />
        </div>
        <h3 class="card-title">{{ $t('profile.appLanguage') }}</h3>
        <select v-model="$i18n.locale" class="language-select">
          <option value="es">Español</option>
          <option value="en">English</option>
        </select>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useProfileStore } from '../../application/profile.store.js'
import { ProfileApi } from '../../infrastructure/profile-api.js'
import PvButton from 'primevue/button'

const { t } = useI18n()
const store = useProfileStore()
const api = new ProfileApi()
const profile = computed(() => store.profile)
const isEditing = ref(false)
const fileInput = ref(null)

function triggerPhotoUpload() {
  if (fileInput.value) {
    fileInput.value.click()
  }
}

function handlePhotoChange(event) {
  const file = event.target.files?.[0]
  if (!file) return

  const reader = new FileReader()
  reader.onload = (e) => {
    profile.value.photoUrl = e.target.result
  }
  reader.readAsDataURL(file)
}

async function toggleEdit() {
  if (isEditing.value) {
    await saveProfile()
  }
  isEditing.value = !isEditing.value
}

async function saveProfile() {
  try {
    // Only send fields that belong to profiles bounded context
    // Email and role are not part of profiles, they belong to IAM
    await api.updateProfile(profile.value.id, {
      id: profile.value.id,
      userId: profile.value.userId,
      name: profile.value.name,
      username: profile.value.username,
      address: profile.value.address,
      phoneNumber: profile.value.phoneNumber,
      photoUrl: profile.value.photoUrl,
      secondEmail: profile.value.secondEmail
    })
    console.log('Profile updated successfully')
    if (profile.value?.userId) {
      await store.fetchProfile(profile.value.userId)
    }
  } catch (error) {
    console.error('Error updating profile:', error)
  }
}

function cancelEdit() {
  isEditing.value = false
  // Use userId from profile, not profile.id
  store.fetchProfile(profile.value.userId)
}
</script>

<style scoped>
* {
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}
.profile-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 20px;
  padding: 2rem;
  background-color: #f8fafc;
}
.profile-title {
  text-align: left;
  font-size: 1.8rem;
  font-weight: 700;
  color: #111827;
  margin-bottom: 1.2rem;
  width: 100%;
  max-width: 800px;
}
.profile-header {
  display: flex;
  justify-content: center;
  width: 100%;
}
.profile-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  background-color: #ecfdf5;
  border-radius: 10px;
  border: 1px solid black;
  padding: 1.5rem 2rem;
  width: 100%;
  max-width: 800px;
}
.photo-wrapper {
  position: relative;
  width: 120px;
  height: 120px;
  flex-shrink: 0;
}
.photo-upload-overlay {
  position: absolute;
  bottom: 0;
  right: 0;
}
.change-photo-btn {
  background-color: #059669 !important;
  color: white !important;
  border: 2px solid white !important;
  width: 36px !important;
  height: 36px !important;
}
.profile-photo {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  border: 2px solid black;
  object-fit: cover;
  display: block;
}
.profile-photo.placeholder {
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2.5rem;
  font-weight: 600;
  color: black;
  background-color: #d1d5db;
}
.profile-info {
  flex-grow: 1;
}
.profile-name {
  font-size: 1.4rem;
  font-weight: 600;
  color: #000;
}
.profile-role {
  font-size: 0.95rem;
  color: #475569;
}
.edit-buttons,
.edit-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}
.edit-button {
  background-color: #10b981;
  color: white;
  border: none;
  font-weight: bold;
}
.cancel-button {
  background-color: #ef4444;
  color: white;
  border: none;
  font-weight: bold;
}
.profile-body {
  width: 100%;
  display: flex;
  justify-content: center;
}
.account-card {
  background: white;
  border-radius: 10px;
  border: 1px solid black;
  padding: 25px;
  width: 100%;
  max-width: 800px;
}
.card-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: black;
  margin-bottom: 20px;
  padding-bottom: 10px;
  border-bottom: 1px solid black;
}
.info-group {
  margin-bottom: 15px;
}
.info-group label {
  display: block;
  font-size: 0.9rem;
  color: #111827;
  margin-bottom: 5px;
  font-weight: 500;
}
.info-input {
  width: 100%;
  background-color: #ecfdf5;
  border: 1px solid black;
  border-radius: 6px;
  padding: 0.6rem 0.8rem;
  font-size: 0.95rem;
  color: #111827;
}
.language-select {
  margin-top: 10px;
  padding: 8px 10px;
  border: 1px solid black;
  border-radius: 6px;
  width: 100%;
}
</style>
