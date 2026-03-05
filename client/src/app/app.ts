import { Component, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { lastValueFrom } from 'rxjs';
import { Nav } from '../layout/nav/nav';

@Component({
  selector: 'app-root',
  imports: [Nav],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private http = inject(HttpClient);
  protected title = 'Dating App';
  protected members = signal<any>([]);

  async ngOnInit(): Promise<void> {
    this.members.set(await this.getMembers())
  }

  async getMembers() {
    try {
      return lastValueFrom(this.http.get('https://localhost:7202/api/members'));
    } catch (error) {
      console.log(error);
      throw error;
    }
  }
}
