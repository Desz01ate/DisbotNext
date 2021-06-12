<template>
  <div id="content">
    <div id="left">
      <h5 class="label">{{ label }}:</h5>
    </div>
    <div id="right">
      <div class="control">
        <slot></slot>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";

export default defineComponent({
  props: {
    label: String,
  },
});
</script>


<style scoped>
* {
  box-sizing: border-box;
}

#content {
  padding: 0.5rem;
  margin: 0;
  width: 100%;
}

#left {
  padding: 0.5rem;
  display: inline-block;
  width: 30%;
}

#right {
  padding: 0.5rem;
  display: inline-block;
  width: 70%;
}

.label {
  text-align: start;
  /* background: lightcoral; */
}

.control {
  display: flex;
  margin-left: 10%;
  /* background: lightgreen; */
}
</style>