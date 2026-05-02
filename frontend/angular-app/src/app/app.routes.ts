import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/tests', pathMatch: 'full' },
  {
    path: 'tests',
    loadComponent: () =>
      import('./components/test-runner/test-runner.component').then(
        (m) => m.TestRunnerComponent,
      ),
  },
  {
    path: 'results',
    loadComponent: () =>
      import('./components/results/results.component').then(
        (m) => m.ResultsComponent,
      ),
  },
  {
    path: 'status',
    loadComponent: () =>
      import('./components/status/status.component').then(
        (m) => m.StatusComponent,
      ),
  },
  {
    path: 'demo',
    loadComponent: () =>
      import('./components/demo/demo.component').then(
        (m) => m.DemoComponent,
      ),
    children: [
      { path: '', redirectTo: 'products', pathMatch: 'full' },
      {
        path: 'products',
        loadComponent: () =>
          import('./components/product-list/product-list.component').then(
            (m) => m.ProductListComponent,
          ),
      },
      {
        path: 'products/:id',
        loadComponent: () =>
          import('./components/product-detail/product-detail.component').then(
            (m) => m.ProductDetailComponent,
          ),
      },
      {
        path: 'streaming',
        loadComponent: () =>
          import('./components/streaming-demo/streaming-demo.component').then(
            (m) => m.StreamingDemoComponent,
          ),
      },
    ],
  },
];
