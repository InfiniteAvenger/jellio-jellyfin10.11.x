import { useEffect } from 'react';
import type { UseFormReturn } from 'react-hook-form';
import { getConfigFromServer } from '@/services/backendService';

const STORAGE_KEY = 'jellio_config';

interface StoredConfig {
  libraries?: Array<{ key: string; name: string; type: string }>;
  jellyseerrEnabled?: boolean;
  jellyseerrUrl?: string;
  jellyseerrApiKey?: string;
  publicBaseUrl?: string;
}

export const useConfigStorage = (form: UseFormReturn<any>, accessToken?: string) => {
  // Load config from localStorage and server on mount
  useEffect(() => {
    const loadConfig = async () => {
      try {
        // First try localStorage (faster)
        const stored = localStorage.getItem(STORAGE_KEY);
        if (stored) {
          const config: StoredConfig = JSON.parse(stored);
          
          // Update form with stored values
          if (config.libraries && Array.isArray(config.libraries)) {
            form.setValue('libraries', config.libraries);
          }
          if (config.jellyseerrEnabled !== undefined) {
            form.setValue('jellyseerrEnabled', config.jellyseerrEnabled);
          }
          if (config.jellyseerrUrl) {
            form.setValue('jellyseerrUrl', config.jellyseerrUrl);
          }
          if (config.jellyseerrApiKey) {
            form.setValue('jellyseerrApiKey', config.jellyseerrApiKey);
          }
          if (config.publicBaseUrl) {
            form.setValue('publicBaseUrl', config.publicBaseUrl);
          }
        }

        // Then try server config (may override localStorage)
        if (accessToken) {
          const serverConfig = await getConfigFromServer(accessToken);
          if (serverConfig) {
            if (serverConfig.jellyseerrEnabled !== undefined) {
              form.setValue('jellyseerrEnabled', serverConfig.jellyseerrEnabled);
            }
            if (serverConfig.jellyseerrUrl) {
              form.setValue('jellyseerrUrl', serverConfig.jellyseerrUrl);
            }
            if (serverConfig.jellyseerrApiKey) {
              form.setValue('jellyseerrApiKey', serverConfig.jellyseerrApiKey);
            }
            if (serverConfig.publicBaseUrl) {
              form.setValue('publicBaseUrl', serverConfig.publicBaseUrl);
            }
          }
        }
      } catch (error) {
        console.error('Failed to load config:', error);
      }
    };

    loadConfig();
  }, [form, accessToken]);

  // Save config to localStorage
  const saveConfig = () => {
    try {
      const values = form.getValues();
      
      // Strip trailing slashes from URLs before saving
      const stripTrailingSlash = (url: string) => url?.replace(/\/+$/, '') || '';
      
      const config: StoredConfig = {
        libraries: values.libraries || [],
        jellyseerrEnabled: values.jellyseerrEnabled,
        jellyseerrUrl: stripTrailingSlash(values.jellyseerrUrl),
        jellyseerrApiKey: values.jellyseerrApiKey,
        publicBaseUrl: stripTrailingSlash(values.publicBaseUrl),
      };
      
      localStorage.setItem(STORAGE_KEY, JSON.stringify(config));
      return true;
    } catch (error) {
      console.error('Failed to save config to localStorage:', error);
      return false;
    }
  };

  return { saveConfig };
};
