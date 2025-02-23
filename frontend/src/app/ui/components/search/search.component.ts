import { Component } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { BaseComponent } from 'src/app/common/ui/base-component';

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css'],
})
export class SearchComponent extends BaseComponent {
  searchControl = new FormControl('');

  constructor() {
    super();
  }
}
