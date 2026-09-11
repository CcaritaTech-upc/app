<template>
  <div class="auth-container">
    <!-- Left Side - Form -->
    <div class="auth-form-side">
      <!-- Green Fragment Decoration -->
      <div class="green-fragment"></div>

      <div class="form-content">
        <div class="form-wrapper">
          <h2 class="form-title">{{ $t('iam.registerOwner.title') }}</h2>
          <p class="form-subtitle">{{ $t('iam.registerOwner.subtitle') || 'Create your owner account to manage your properties.' }}</p>

          <pv-stepper v-model:value="currentStep" linear>
            <pv-step-list>
              <pv-step :value="1">{{ $t('iam.registerOwner.userInfoSection') }}</pv-step>
              <pv-step :value="2">{{ $t('iam.registerOwner.profileInfoSection') }}</pv-step>
            </pv-step-list>

            <pv-step-panels>
              <!-- Step 1: Account Information -->
              <pv-step-panel :value="1">
                <div class="step-content">
          <!-- Email -->
          <div class="mb-3">
            <label for="email" class="block mb-2">{{ $t('iam.registerOwner.email') }} *</label>
            <pv-input-text
              id="email"
              v-model="registerForm.email"
              type="email"
              :placeholder="$t('iam.registerOwner.emailPlaceholder')"
              class="w-full"
            />
          </div>

          <!-- Password -->
          <div class="mb-3">
            <label for="password" class="block mb-2">{{ $t('iam.registerOwner.password') }} *</label>
            <pv-password
              id="password"
              v-model="registerForm.password"
              :placeholder="$t('iam.registerOwner.passwordPlaceholder')"
              toggleMask
              class="w-full"
              inputClass="w-full"
            />
          </div>

          <!-- Confirm Password -->
          <div class="mb-3">
            <label for="confirmPassword" class="block mb-2">{{ $t('iam.registerOwner.confirmPassword') }} *</label>
            <pv-password
              id="confirmPassword"
              v-model="registerForm.confirmPassword"
              :placeholder="$t('iam.registerOwner.confirmPasswordPlaceholder')"
              :feedback="false"
              toggleMask
              class="w-full"
              inputClass="w-full"
            />
          </div>

          <!-- Error Message -->
          <pv-message v-if="errorMessage" severity="error" :closable="false" class="mb-3">
            {{ errorMessage }}
          </pv-message>

                <div class="flex gap-2 pt-4">
                  <pv-button
                    type="button"
                    :label="$t('iam.registerOwner.cancelButton')"
                    severity="secondary"
                    outlined
                    @click="goToLogin"
                    class="flex-1"
                  />
                  <pv-button
                    :label="'Next'"
                    icon="pi pi-arrow-right"
                    iconPos="right"
                    @click="goToStep2"
                    class="flex-1"
                  />
                </div>
              </div>
            </pv-step-panel>

            <!-- Step 2: Personal Information -->
            <pv-step-panel :value="2">
              <div class="step-content">
                <!-- Builder assignment detection banner -->
                <pv-message v-if="invitationInfo" severity="success" :closable="false" class="mb-4">
                  <div class="flex items-center gap-2">
                    <i class="pi pi-check-circle" style="font-size: 1.1rem;"></i>
                    <span>
                      ¡Asignación detectada! Tu cuenta se vinculará a la unidad <strong>{{ invitationInfo.unitNumber }}</strong> en <strong>{{ invitationInfo.projectName }}</strong>.
                    </span>
                  </div>
                </pv-message>

          <!-- Photo URL -->
          <div class="mb-3">
            <label for="photoUrl" class="block mb-2">{{ $t('iam.registerOwner.photoUrl') }}</label>
            <input
              type="file"
              ref="fileInput"
              accept="image/*"
              style="display: none;"
              @change="handleLocalFileUpload"
            />
            <pv-button
              type="button"
              :label="registerForm.photoUrl ? 'Cambiar Foto' : $t('iam.registerOwner.uploadPhoto')"
              icon="pi pi-cloud-upload"
              @click="openUploadModal"
              severity="secondary"
              outlined
              class="w-full mb-2"
            />
            <div v-if="registerForm.photoUrl" class="mt-2 text-center">
              <img
                :src="registerForm.photoUrl"
                alt="Profile photo preview"
                class="uploaded-image"
              />
              <div class="flex justify-content-center align-items-center gap-2 mt-2">
                <span class="text-sm text-green-600 font-medium">✓ Imagen seleccionada</span>
                <pv-button
                  type="button"
                  icon="pi pi-trash"
                  text
                  rounded
                  severity="danger"
                  size="small"
                  @click="registerForm.photoUrl = ''"
                  title="Eliminar foto"
                />
              </div>
            </div>
          </div>

          <!-- Name -->
          <div class="mb-3">
            <label for="name" class="block mb-2">{{ $t('iam.registerOwner.name') }} *</label>
            <pv-input-text
              id="name"
              v-model="registerForm.name"
              :placeholder="$t('iam.registerOwner.namePlaceholder')"
              class="w-full"
            />
          </div>

          <!-- Username -->
          <div class="mb-3">
            <label for="username" class="block mb-2">{{ $t('iam.registerOwner.username') }} *</label>
            <pv-input-text
              id="username"
              v-model="registerForm.username"
              :placeholder="$t('iam.registerOwner.usernamePlaceholder')"
              class="w-full"
            />
          </div>

          <!-- Address -->
          <div class="mb-3">
            <label for="address" class="block mb-2">{{ $t('iam.registerOwner.address') }} *</label>
            <pv-input-text
              id="address"
              v-model="registerForm.address"
              :placeholder="$t('iam.registerOwner.addressPlaceholder')"
              class="w-full"
            />
          </div>

          <!-- Age -->
          <div class="mb-3">
            <label for="age" class="block mb-2">{{ $t('iam.registerOwner.age') }} *</label>
            <pv-input-number
              id="age"
              v-model="registerForm.age"
              :min="18"
              :max="100"
              :placeholder="$t('iam.registerOwner.agePlaceholder')"
              class="w-full"
            />
          </div>

          <!-- Phone Number -->
          <div class="mb-3">
            <label for="phoneNumber" class="block mb-2">{{ $t('iam.registerOwner.phoneNumber') }} *</label>
            <pv-input-text
              id="phoneNumber"
              v-model="registerForm.phoneNumber"
              :placeholder="$t('iam.registerOwner.phoneNumberPlaceholder')"
              class="w-full"
            />
          </div>

          <!-- Error/Success Messages -->
          <pv-message v-if="errorMessage" severity="error" :closable="false" class="mb-3">
            {{ errorMessage }}
          </pv-message>

          <pv-message v-if="successMessage" severity="success" :closable="false" class="mb-3">
            {{ successMessage }}
          </pv-message>

                <div class="flex gap-2 pt-4">
                  <pv-button
                    :label="'Back'"
                    severity="secondary"
                    icon="pi pi-arrow-left"
                    @click="currentStep = 1"
                    class="flex-1"
                  />
                  <pv-button
                    :label="$t('iam.registerOwner.submitButton')"
                    icon="pi pi-check"
                    :loading="isLoading"
                    @click="handleRegister"
                    class="flex-1"
                  />
                </div>
              </div>
            </pv-step-panel>
          </pv-step-panels>
        </pv-stepper>
        </div>
      </div>
    </div>

    <!-- Right Side - Image -->
    <div class="auth-image-side"></div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useIamStore } from '../../application/iam.store.js';
import { useProfileStore } from '../../../profiles/application/profile.store.js';
import { IamApi } from '../../infrastructure/iam-api.js';

const router = useRouter();
const iamStore = useIamStore();
const profileStore = useProfileStore();
const iamApi = new IamApi();

const currentStep = ref(1);

import { CLOUDINARY_WIDGET_URL } from "../../../shared/infrastructure/constants.js";
import { getAvatarUploadConfig } from "../../../shared/infrastructure/cloudinary-config.js";

// Cloudinary configuration
const cloudinaryName = import.meta.env.VITE_CLOUDINARY_CLOUD_NAME;
const cloudinaryPreset = import.meta.env.VITE_CLOUDINARY_UPLOAD_PRESET;
const cloudinaryReady = ref(false);

onMounted(() => {
  loadCloudinaryScript();
});

const loadCloudinaryScript = () => {
  if (window.cloudinary) {
    cloudinaryReady.value = true;
    return;
  }
  const script = document.createElement('script');
  script.src = CLOUDINARY_WIDGET_URL;
  script.type = 'text/javascript';
  script.onload = () => { cloudinaryReady.value = true; };
  document.head.appendChild(script);
};

const fileInput = ref(null);

const handleLocalFileUpload = (event) => {
  const file = event.target.files?.[0];
  if (!file) return;
  const reader = new FileReader();
  reader.onload = (e) => {
    registerForm.value.photoUrl = e.target.result;
  };
  reader.readAsDataURL(file);
};

const openUploadModal = () => {
  const hasCloudinary = cloudinaryName && cloudinaryName.trim() !== '' && cloudinaryPreset && cloudinaryPreset.trim() !== '';
  if (hasCloudinary && cloudinaryReady.value && window.cloudinary) {
    try {
      const widgetConfig = getAvatarUploadConfig(cloudinaryName, cloudinaryPreset);
      window.cloudinary.openUploadWidget(
        widgetConfig,
        (error, result) => {
          if (!error && result && result.event === "success") {
            registerForm.value.photoUrl = result.info.secure_url || result.info.url;
          }
        }
      );
      return;
    } catch (err) {
      console.warn('Cloudinary widget failed, using local file picker:', err);
    }
  }

  // Fallback seguro: abrir selector de archivo nativo sin depender de Cloudinary
  if (fileInput.value) {
    fileInput.value.click();
  }
};

const registerForm = ref({
  email: '',
  password: '',
  confirmPassword: '',
  photoUrl: '',
  name: '',
  username: '',
  address: '',
  age: null,
  phoneNumber: ''
});

const isLoading = ref(false);
const errorMessage = ref('');
const successMessage = ref('');
const invitationInfo = ref(null);
const checkingInvitation = ref(false);

async function checkBuilderAssignment(email) {
  if (!email || !email.includes('@')) return;
  try {
    checkingInvitation.value = true;
    const res = await iamApi.checkInvitation(email.trim());
    if (res?.data?.assigned) {
      invitationInfo.value = res.data;
      if (!registerForm.value.name && res.data.fullName) {
        registerForm.value.name = res.data.fullName;
      }
      if (!registerForm.value.phoneNumber && res.data.phoneNumber) {
        registerForm.value.phoneNumber = res.data.phoneNumber;
      }
      if (!registerForm.value.address) {
        registerForm.value.address = res.data.address || (res.data.unitNumber ? `Unidad ${res.data.unitNumber}` : '');
      }
    } else {
      invitationInfo.value = null;
    }
  } catch (err) {
    console.debug('Invitation lookup failed or none found:', err);
  } finally {
    checkingInvitation.value = false;
  }
}

function goToStep2() {
  errorMessage.value = '';

  // Validate required fields
  if (!registerForm.value.email || !registerForm.value.password || !registerForm.value.confirmPassword) {
    errorMessage.value = 'Please fill in all required fields';
    return;
  }

  // Validate email format
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!emailRegex.test(registerForm.value.email)) {
    errorMessage.value = 'Please enter a valid email address';
    return;
  }

  // Validate passwords match
  if (registerForm.value.password !== registerForm.value.confirmPassword) {
    errorMessage.value = 'Passwords do not match';
    return;
  }

  // Validate password strength
  if (registerForm.value.password.length < 6) {
    errorMessage.value = 'Password must be at least 6 characters long';
    return;
  }

  // All validations passed, move to next step and check for builder assignments
  currentStep.value = 2;
  checkBuilderAssignment(registerForm.value.email);
}

async function handleRegister() {
  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  // Validate required fields for step 2
  if (!registerForm.value.name || !registerForm.value.username || !registerForm.value.address ||
      !registerForm.value.age || !registerForm.value.phoneNumber) {
    errorMessage.value = 'Please fill in all required fields';
    isLoading.value = false;
    return;
  }

  // Validate age
  if (registerForm.value.age < 18 || registerForm.value.age > 100) {
    errorMessage.value = 'Age must be between 18 and 100';
    isLoading.value = false;
    return;
  }

  try {
    // 1. Create user with role "Owner"
    console.log('Step 1: Creating user account...');
    await iamStore.signUp({
      email: registerForm.value.email,
      password: registerForm.value.password,
      role: 'Owner'
    });
    console.log('Step 1: User account created successfully');

    // 2. Sign in to get the user ID
    console.log('Step 2: Signing in to get user ID...');
    const authenticatedUser = await iamStore.signIn(registerForm.value.email, registerForm.value.password);
    console.log('Step 2: Sign in successful, user ID:', authenticatedUser.id);

    // Validate that we have a user ID
    if (!authenticatedUser || !authenticatedUser.id) {
      throw new Error('Failed to retrieve user ID after sign in');
    }

    // 3. Create profile with user ID
    console.log('Step 3: Creating profile for user ID:', authenticatedUser.id);
    const profileData = {
      userId: authenticatedUser.id,
      photoUrl: registerForm.value.photoUrl || '',
      name: registerForm.value.name,
      username: registerForm.value.username,
      address: registerForm.value.address,
      age: registerForm.value.age ? parseInt(registerForm.value.age) : null,
      phoneNumber: registerForm.value.phoneNumber,
      secondEmail: registerForm.value.secondEmail || ''
    };
    console.log('Profile data to be created:', profileData);
    
    await profileStore.createProfile(profileData);
    console.log('Step 3: Profile created successfully');

    // Immediately update IAM user in store and localStorage so layout header reflects name and photo
    iamStore.updateUserProfile({
      username: profileData.username,
      name: profileData.name,
      photoUrl: profileData.photoUrl
    });

    successMessage.value = 'Registration successful! Redirecting...';
    
    // Redirect to home after 2 seconds
    setTimeout(() => {
      router.push({ name: 'home' });
    }, 2000);

  } catch (error) {
    console.error('Registration error:', error);
    console.error('Error details:', {
      message: error.message,
      response: error.response?.data,
      status: error.response?.status
    });
    
    // Provide more specific error messages
    if (error.message.includes('user ID')) {
      errorMessage.value = 'Failed to complete registration. Please try logging in manually.';
    } else if (error.response?.status === 409) {
      errorMessage.value = 'Email already exists. Please try logging in instead.';
    } else {
      errorMessage.value = error.response?.data?.message || error.message || 'Registration failed. Please try again.';
    }
  } finally {
    isLoading.value = false;
  }
}

function goToLogin() {
  router.push({ name: 'login' });
}
</script>

<style scoped>
.auth-container {
  display: flex;
  min-height: 100vh;
  width: 100%;
  background-color: #262626;
}

/* Left Side - Form */
.auth-form-side {
  flex: 1;
  background-color: #18181b;
  background-image: 
      linear-gradient(rgba(255, 255, 255, 0.02) 1px, transparent 1px),
      linear-gradient(90deg, rgba(255, 255, 255, 0.02) 1px, transparent 1px);
  background-size: 40px 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 2rem;
  overflow-y: auto;
  position: relative;
  z-index: 10;
}

/* Green Fragment Decoration */
.green-fragment {
  position: absolute;
  bottom: 0;
  right: 0;
  width: 120px;
  height: 300px;
  background: linear-gradient(to top, #10b981, transparent);
  clip-path: polygon(100% 0, 100% 100%, 0% 100%);
  opacity: 0.6;
  z-index: -1;
}

.form-content {
  width: 100%;
  max-width: 28rem;
  position: relative;
  z-index: 1;
}

.form-wrapper {
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(10px);
  padding: 2rem;
  border-radius: 1rem;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
}

.form-title {
  font-size: 1.875rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0 0 0.5rem 0;
}

.form-subtitle {
  color: #6b7280;
  margin: 0 0 1.5rem 0;
  font-size: 0.95rem;
}

.step-content {
  padding: 1rem 0;
}

.mb-3 {
  margin-bottom: 1.25rem;
}

.uploaded-image {
  max-width: 200px;
  max-height: 200px;
  border-radius: 8px;
  border: 2px solid #10b981;
  object-fit: cover;
  margin: 0 auto;
  display: block;
}

/* Right Side - Image */
.auth-image-side {
  flex: 1;
  background-image: url('https://img.freepik.com/free-photo/modern-business-building-with-glass-wall-from-empty-floor_1127-3091.jpg?semt=ais_hybrid&w=740&q=80');
  background-size: cover;
  background-position: center;
  clip-path: polygon(10% 0, 100% 0, 100% 100%, 0% 100%);
}

/* Responsive Design */
@media (max-width: 968px) {
  .auth-container {
    flex-direction: column;
  }

  .auth-image-side {
    min-height: 300px;
    order: -1;
    clip-path: polygon(0 0, 100% 0, 100% 85%, 0 100%);
  }

  .green-fragment {
    display: none;
  }
}

@media (max-width: 640px) {
  .auth-form-side {
    padding: 1rem;
  }

  .form-wrapper {
    padding: 1.5rem;
  }

  .form-title {
    font-size: 1.5rem;
  }
}
</style>
