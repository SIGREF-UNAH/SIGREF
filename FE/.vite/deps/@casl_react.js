import {
  require_react
} from "./chunk-LTZIYYLR.js";
import {
  __toESM
} from "./chunk-V4OQ3NZ2.js";

// node_modules/@casl/react/dist/es6m/index.mjs
var import_react = __toESM(require_react(), 1);
var s = () => {
};
var e = class extends import_react.PureComponent {
  constructor(...t2) {
    super(...t2);
    this.i = false;
    this.h = null;
    this.o = s;
    this.state = { t: true };
  }
  componentWillUnmount() {
    this.o();
  }
  u(t2) {
    if (t2 === this.h) return;
    this.o();
    this.h = null;
    if (t2) {
      this.h = t2;
      this.o = t2.on("updated", (() => this.setState({ t: !this.state.t })));
    }
  }
  get allowed() {
    return this.i;
  }
  l() {
    const t2 = this.props;
    const i2 = t2.of || t2.a || t2.an || t2.this || t2.on;
    const n2 = t2.not ? "cannot" : "can";
    return t2.ability[n2](t2.I || t2.do, i2, t2.field);
  }
  render() {
    this.u(this.props.ability);
    this.i = this.l();
    return this.props.passThrough || this.i ? this.p() : null;
  }
  p() {
    const { children: t2, ability: i2 } = this.props;
    const n2 = typeof t2 === "function" ? t2(this.i, i2) : t2;
    return n2;
  }
};
function r(t2) {
  return (i2) => (0, import_react.createElement)(t2, { children: (t3) => (0, import_react.createElement)(e, Object.assign({}, i2, { ability: i2.ability || t3 })) });
}
function useAbility(i2) {
  const n2 = import_react.default.useContext(i2);
  const [s2, e2] = import_react.default.useState();
  import_react.default.useEffect((() => n2.on("updated", ((t2) => {
    if (t2.rules !== s2) e2(t2.rules);
  }))), []);
  return n2;
}
export {
  e as Can,
  r as createContextualCan,
  useAbility
};
//# sourceMappingURL=@casl_react.js.map
