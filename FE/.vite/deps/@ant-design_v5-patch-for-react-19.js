"use client";
import {
  require_client
} from "./chunk-WIOBCSLY.js";
import {
  unstableSetRender
} from "./chunk-XLFHVX7M.js";
import "./chunk-CDDPM2PX.js";
import "./chunk-AZZLBHVG.js";
import "./chunk-N77XPGZE.js";
import "./chunk-XFICPL23.js";
import "./chunk-QDJUJEL2.js";
import "./chunk-NBRNFAZM.js";
import "./chunk-ZMA7LKPH.js";
import "./chunk-77QR7T7N.js";
import "./chunk-UZBTDLRM.js";
import "./chunk-Z54NHHXK.js";
import "./chunk-LTZIYYLR.js";
import {
  __toESM
} from "./chunk-V4OQ3NZ2.js";

// node_modules/@ant-design/v5-patch-for-react-19/es/index.js
var import_client = __toESM(require_client());
unstableSetRender(function(node, container) {
  container._reactRoot || (container._reactRoot = (0, import_client.createRoot)(container));
  var root = container._reactRoot;
  root.render(node);
  return function() {
    return new Promise(function(resolve) {
      setTimeout(function() {
        root.unmount();
        resolve();
      }, 0);
    });
  };
});
//# sourceMappingURL=@ant-design_v5-patch-for-react-19.js.map
