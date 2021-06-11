<template>
  <div class="about">
    <h1>DisbotNext Configuration</h1>
    <div>
      <h3>Bot Token: <input type="password" v-model="botToken" /></h3>
      <h3>Report Cron: <input type="text" v-model="reportCron" /></h3>
    </div>
    <br />
    <div>
      <h3>
        Save Changes:
        <button class="btn btn-primary" v-on:click="saveChangesAsync">
          Save
        </button>
      </h3>
      <h3>
        Restart Service:
        <button class="btn btn-danger" disabled v-on:click="restartServiceAsync">
          Restart
        </button>
      </h3>
    </div>
  </div>
</template>

<script>
import { defineComponent } from "vue";

export default defineComponent({
  computed: {
    botToken: {
      get() {
        return this.$store.state.botToken;
      },
      set(value) {
        this.$store.dispatch("updateBotToken", value);
      },
    },
    reportCron: {
      get() {
        return this.$store.state.reportCron;
      },
      set(value) {
        this.$store.dispatch("updateReportCron", value);
      },
    },
  },
  mounted() {
    this.fetchBotToken();
    this.fetchReportCron();
  },
  methods: {
    fetchBotToken() {
      return this.$store.dispatch("fetchBotToken");
    },
    fetchReportCron() {
      return this.$store.dispatch("fetchReportCron");
    },
    async saveChangesAsync() {
      const result = await this.$store.dispatch("saveChanges");
      if (result) {
        alert(
          "Changes has been saved, you will need to restart to apply changes."
        );
      } else {
        alert("Unable to save changes to server.");
      }
    },
    async restartServiceAsync() {
      if (confirm("Do you want to restart the service?")) {
        this.$store.dispatch("restartService");
      }
    },
  },
});
</script>
