import { createStore } from 'vuex'
import axios from 'axios'

const ApiEndpoint = 'https://localhost:5001';

export default createStore({
  state: {
    botToken: '',
    reportCron: ''
  },
  mutations: {
    updateBotTokenValue: (state, token) => {
      state.botToken = token;
    },
    updateReportCronValue: (state, cron) => {
      state.reportCron = cron;
    }
  },
  actions: {
    fetchBotToken: async (context) => {
      const result = await axios.get(`${ApiEndpoint}/api/configuration/BotToken`);
      const token = result.data;
      context.commit('updateBotTokenValue', token);
    },
    updateBotToken: async (context, token) => {
      context.commit('updateBotTokenValue', token);
    },
    fetchReportCron: async (context) => {
      const result = await axios.get(`${ApiEndpoint}/api/configuration/ReportCron`);
      const cron = result.data;
      console.log(cron);
      context.commit('updateReportCronValue', cron);
    },
    updateReportCron: async (context, cron) => {
      context.commit('updateReportCronValue', cron);
    },
    saveChanges: async (context): Promise<boolean> => {
      const botTokenUpdateResult = await axios.put(`${ApiEndpoint}/api/configuration/BotToken/${context.state.botToken}`);
      if (botTokenUpdateResult.status != 200 && botTokenUpdateResult.status != 204) {
        return false;
      }

      const cronUpdateResult = await axios.put(`${ApiEndpoint}/api/configuration/ReportCron/${context.state.reportCron}`);
      if (cronUpdateResult.status != 200 && cronUpdateResult.status != 204) {
        return false;
      }

      return true;
    },
    restartService: async (context): Promise<boolean> => {
      const restartResult = await axios.post(`${ApiEndpoint}/api/configuration/restart`);
      return restartResult.data;
    }
  },
  modules: {
  }
})
