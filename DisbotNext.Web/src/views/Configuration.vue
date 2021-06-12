<template>
  <div class="about">
    <h1 style="text-align: left; margin: 10px">DisbotNext Configuration</h1>
    <br />
    <config-row label="Bot Token">
      <input type="password" class="input-width" v-model="botToken" />
    </config-row>
    <config-row label="Report Cron"
      ><input type="text" class="input-width" v-model="reportCron" />
    </config-row>
    <config-row label="Save Changes">
      <button
        class="btn btn-primary btn-width"
        v-on:click="saveChangesAsync"
      >
        Save
      </button>
    </config-row>
    <config-row label="Restart Service">
      <button
        class="btn btn-danger btn-width"
        disabled
        v-on:click="restartServiceAsync"
      >
        Restart
      </button>
    </config-row>
  </div>
</template>

<script>
import { defineComponent } from "vue";
import ConfigRow from "../components/ConfigRow.vue";

export default defineComponent({
  components: {
    ConfigRow,
  },
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

<style lang="postcss" scoped>
.btn-width {
  width: 100px;
}

.input-width {
  width: 500px;
}
</style>