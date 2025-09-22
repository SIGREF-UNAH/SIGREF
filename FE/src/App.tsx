import { BrowserRouter } from "react-router";
import { AppRouter } from "./routers";
import { AntdConfig } from "./config";

function App() {
  return (
    <AntdConfig>
      <BrowserRouter>
        <AppRouter />
      </BrowserRouter>
    </AntdConfig>
  );
}

export default App;
