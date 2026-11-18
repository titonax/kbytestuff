// Synthetic parser fixture; contains no extracted Wand code.
// Method and private-field names are deliberately renamed by the minified test.
const app = { whenReady: () => Promise.resolve() };
function run() {}
app.whenReady().then(run);

const handlers = {
  registerHandler() {},
  registerReducer() {},
};
handlers.registerHandler("ACTION_CHECK_FOR_UPDATE", () => true);
handlers.registerReducer("ACTION_SET_ACCOUNT", function reduce(state, account) {
  return { ...state, account: account };
});

class SyntheticAccount {
  getUserAccount() {
    return Promise.resolve({ id: "fixture" });
  }
  setAccountWandBrandExperience() {
    const endpoint = "/v3/account/brand_experience_wand";
    return Promise.resolve({ endpoint });
  }
  setAccountLanguage() {
    return Promise.resolve({ language: "fixture" });
  }
  requestRemoteAuthCode() {
    return Promise.resolve("fixture");
  }
}

class SyntheticRemote {
  #trainer;
  #trainerId;
  #timestamp;
  transport;
  session;
  status;

  setCurrentTrainer(info) {
    this.#trainer = info.trainer;
  }

  #setValue(change) {
    return this.#trainer.setValue(change.name, change.value, "remote", change.cheatId);
  }

  subscribe() {
    this.transport.listen("client-value-changed", change => this.#setValue(change));
    this.#trainer.onValueSet(change => {
      this.transport.send("client-value-changed", { name: change.name, value: change.value });
    });
  }

  #reset() {
    this.#timestamp = Date.now();
  }

  #sendState() {
    const metadata = this.#trainer?.getMetadata(this.session);
    if (this.status === "connected") {
      this.transport.send("client-state", { trainerId: this.#trainerId, metadata });
    }
  }
}
