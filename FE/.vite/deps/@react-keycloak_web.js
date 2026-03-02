import {
  require_react
} from "./chunk-LTZIYYLR.js";
import {
  __commonJS,
  __toESM
} from "./chunk-V4OQ3NZ2.js";

// node_modules/react-fast-compare/index.js
var require_react_fast_compare = __commonJS({
  "node_modules/react-fast-compare/index.js"(exports, module) {
    var hasElementType = typeof Element !== "undefined";
    var hasMap = typeof Map === "function";
    var hasSet = typeof Set === "function";
    var hasArrayBuffer = typeof ArrayBuffer === "function" && !!ArrayBuffer.isView;
    function equal(a, b) {
      if (a === b) return true;
      if (a && b && typeof a == "object" && typeof b == "object") {
        if (a.constructor !== b.constructor) return false;
        var length, i, keys;
        if (Array.isArray(a)) {
          length = a.length;
          if (length != b.length) return false;
          for (i = length; i-- !== 0; )
            if (!equal(a[i], b[i])) return false;
          return true;
        }
        var it;
        if (hasMap && a instanceof Map && b instanceof Map) {
          if (a.size !== b.size) return false;
          it = a.entries();
          while (!(i = it.next()).done)
            if (!b.has(i.value[0])) return false;
          it = a.entries();
          while (!(i = it.next()).done)
            if (!equal(i.value[1], b.get(i.value[0]))) return false;
          return true;
        }
        if (hasSet && a instanceof Set && b instanceof Set) {
          if (a.size !== b.size) return false;
          it = a.entries();
          while (!(i = it.next()).done)
            if (!b.has(i.value[0])) return false;
          return true;
        }
        if (hasArrayBuffer && ArrayBuffer.isView(a) && ArrayBuffer.isView(b)) {
          length = a.length;
          if (length != b.length) return false;
          for (i = length; i-- !== 0; )
            if (a[i] !== b[i]) return false;
          return true;
        }
        if (a.constructor === RegExp) return a.source === b.source && a.flags === b.flags;
        if (a.valueOf !== Object.prototype.valueOf && typeof a.valueOf === "function" && typeof b.valueOf === "function") return a.valueOf() === b.valueOf();
        if (a.toString !== Object.prototype.toString && typeof a.toString === "function" && typeof b.toString === "function") return a.toString() === b.toString();
        keys = Object.keys(a);
        length = keys.length;
        if (length !== Object.keys(b).length) return false;
        for (i = length; i-- !== 0; )
          if (!Object.prototype.hasOwnProperty.call(b, keys[i])) return false;
        if (hasElementType && a instanceof Element) return false;
        for (i = length; i-- !== 0; ) {
          if ((keys[i] === "_owner" || keys[i] === "__v" || keys[i] === "__o") && a.$$typeof) {
            continue;
          }
          if (!equal(a[keys[i]], b[keys[i]])) return false;
        }
        return true;
      }
      return a !== a && b !== b;
    }
    module.exports = function isEqual2(a, b) {
      try {
        return equal(a, b);
      } catch (error) {
        if ((error.message || "").match(/stack|recursion/i)) {
          console.warn("react-fast-compare cannot handle circular refs");
          return false;
        }
        throw error;
      }
    };
  }
});

// node_modules/@react-keycloak/core/lib/context.js
var import_react = __toESM(require_react());
var __assign = function() {
  __assign = Object.assign || function(t) {
    for (var s, i = 1, n = arguments.length; i < n; i++) {
      s = arguments[i];
      for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
        t[p] = s[p];
    }
    return t;
  };
  return __assign.apply(this, arguments);
};
function createAuthContext(initialContext) {
  return (0, import_react.createContext)(__assign({ initialized: false }, initialContext));
}

// node_modules/@react-keycloak/core/lib/provider.js
var React = __toESM(require_react());
var import_react_fast_compare = __toESM(require_react_fast_compare());
var __extends = /* @__PURE__ */ (function() {
  var extendStatics = function(d, b) {
    extendStatics = Object.setPrototypeOf || { __proto__: [] } instanceof Array && function(d2, b2) {
      d2.__proto__ = b2;
    } || function(d2, b2) {
      for (var p in b2) if (b2.hasOwnProperty(p)) d2[p] = b2[p];
    };
    return extendStatics(d, b);
  };
  return function(d, b) {
    extendStatics(d, b);
    function __() {
      this.constructor = d;
    }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
  };
})();
var __assign2 = function() {
  __assign2 = Object.assign || function(t) {
    for (var s, i = 1, n = arguments.length; i < n; i++) {
      s = arguments[i];
      for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
        t[p] = s[p];
    }
    return t;
  };
  return __assign2.apply(this, arguments);
};
function createAuthProvider(AuthContext) {
  var defaultInitOptions = {
    onLoad: "check-sso"
  };
  var initialState = {
    initialized: false,
    isAuthenticated: false,
    isLoading: true
  };
  return (
    /** @class */
    (function(_super) {
      __extends(KeycloakProvider, _super);
      function KeycloakProvider() {
        var _this = _super !== null && _super.apply(this, arguments) || this;
        _this.state = __assign2({}, initialState);
        _this.onError = function(event) {
          return function(error) {
            var onEvent = _this.props.onEvent;
            onEvent && onEvent(event, error);
          };
        };
        _this.updateState = function(event) {
          return function() {
            var _a = _this.props, authClient = _a.authClient, onEvent = _a.onEvent, onTokens = _a.onTokens, isLoadingCheck = _a.isLoadingCheck;
            var _b = _this.state, prevInitialized = _b.initialized, prevAuthenticated = _b.isAuthenticated, prevLoading = _b.isLoading;
            onEvent && onEvent(event);
            var isLoading = isLoadingCheck ? isLoadingCheck(authClient) : false;
            var isAuthenticated = isUserAuthenticated(authClient);
            if (!prevInitialized || isAuthenticated !== prevAuthenticated || isLoading !== prevLoading) {
              _this.setState({
                initialized: true,
                isAuthenticated,
                isLoading
              });
            }
            var idToken = authClient.idToken, refreshToken = authClient.refreshToken, token = authClient.token;
            onTokens && onTokens({
              idToken,
              refreshToken,
              token
            });
          };
        };
        _this.refreshToken = function(event) {
          return function() {
            var _a = _this.props, autoRefreshToken = _a.autoRefreshToken, authClient = _a.authClient, onEvent = _a.onEvent;
            onEvent && onEvent(event);
            if (autoRefreshToken !== false) {
              authClient.updateToken(5);
            }
          };
        };
        return _this;
      }
      KeycloakProvider.prototype.componentDidMount = function() {
        this.init();
      };
      KeycloakProvider.prototype.componentDidUpdate = function(_a) {
        var prevAuthClient = _a.authClient, prevInitOptions = _a.initOptions;
        var _b = this.props, initOptions = _b.initOptions, authClient = _b.authClient;
        if (authClient !== prevAuthClient || !(0, import_react_fast_compare.default)(initOptions, prevInitOptions)) {
          prevAuthClient.onReady = void 0;
          prevAuthClient.onAuthSuccess = void 0;
          prevAuthClient.onAuthError = void 0;
          prevAuthClient.onAuthRefreshSuccess = void 0;
          prevAuthClient.onAuthRefreshError = void 0;
          prevAuthClient.onAuthLogout = void 0;
          prevAuthClient.onTokenExpired = void 0;
          this.setState(__assign2({}, initialState));
          this.init();
        }
      };
      KeycloakProvider.prototype.init = function() {
        var _a = this.props, initOptions = _a.initOptions, authClient = _a.authClient;
        authClient.onReady = this.updateState("onReady");
        authClient.onAuthSuccess = this.updateState("onAuthSuccess");
        authClient.onAuthError = this.onError("onAuthError");
        authClient.onAuthRefreshSuccess = this.updateState("onAuthRefreshSuccess");
        authClient.onAuthRefreshError = this.onError("onAuthRefreshError");
        authClient.onAuthLogout = this.updateState("onAuthLogout");
        authClient.onTokenExpired = this.refreshToken("onTokenExpired");
        authClient.init(__assign2(__assign2({}, defaultInitOptions), initOptions)).catch(this.onError("onInitError"));
      };
      KeycloakProvider.prototype.render = function() {
        var _a = this.props, children = _a.children, authClient = _a.authClient, LoadingComponent = _a.LoadingComponent;
        var _b = this.state, initialized = _b.initialized, isLoading = _b.isLoading;
        if (!!LoadingComponent && (!initialized || isLoading)) {
          return LoadingComponent;
        }
        return React.createElement(AuthContext.Provider, { value: { initialized, authClient } }, children);
      };
      return KeycloakProvider;
    })(React.PureComponent)
  );
}
function isUserAuthenticated(authClient) {
  return !!authClient.idToken && !!authClient.token;
}

// node_modules/@react-keycloak/web/lib/context.js
var reactKeycloakWebContext = createAuthContext();
var ReactKeycloakWebContextConsumer = reactKeycloakWebContext.Consumer;

// node_modules/@react-keycloak/web/lib/provider.js
var ReactKeycloakProvider = createAuthProvider(reactKeycloakWebContext);

// node_modules/@react-keycloak/web/lib/useKeycloak.js
var import_react2 = __toESM(require_react());
function useKeycloak() {
  var ctx = (0, import_react2.useContext)(reactKeycloakWebContext);
  if (!ctx) {
    throw new Error("useKeycloak hook must be used inside ReactKeycloakProvider context");
  }
  if (!ctx.authClient) {
    throw new Error("authClient has not been assigned to ReactKeycloakProvider");
  }
  var authClient = ctx.authClient, initialized = ctx.initialized;
  return {
    initialized,
    keycloak: authClient
  };
}

// node_modules/@react-keycloak/web/lib/withKeycloak.js
var React2 = __toESM(require_react());
var __assign3 = function() {
  __assign3 = Object.assign || function(t) {
    for (var s, i = 1, n = arguments.length; i < n; i++) {
      s = arguments[i];
      for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p))
        t[p] = s[p];
    }
    return t;
  };
  return __assign3.apply(this, arguments);
};
function withKeycloak(Component) {
  return function WrappedComponent(props) {
    var _a = useKeycloak(), keycloak = _a.keycloak, initialized = _a.initialized;
    return React2.createElement(Component, __assign3({}, props, { keycloakInitialized: initialized, keycloak }));
  };
}
export {
  ReactKeycloakProvider,
  useKeycloak,
  withKeycloak
};
//# sourceMappingURL=@react-keycloak_web.js.map
